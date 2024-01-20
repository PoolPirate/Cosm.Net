import {
  readFileSync,
  existsSync,
  rm,
  cpSync,
  lstatSync,
  readdirSync,
  rmSync,
  mkdirSync,
} from "node:fs";
import { ExecException, exec } from "node:child_process";
import path from "node:path";
import { ProtoChain, ProtoDir, Repo, loadProtoChainFromFile } from "./repos";

function validateProtoChain(protoChain: ProtoChain) {
  if (!existsSync(path.join(protoChain.repoDir, protoChain.chainRepoName))) {
    console.error(
      `Given chain repo does not exist at ${path.join(
        protoChain.repoDir,
        protoChain.chainRepoName
      )}`
    );
    return null;
  }
  if (!existsSync(protoChain.protoDir)) {
    console.error("Given protoDir does not exist");
    return null;
  }
}

async function main(configPath: string) {
  const protoChain = loadProtoChainFromFile(configPath);
  validateProtoChain(protoChain);
  console.log(`Clearing proto directory at ${protoChain.protoDir}`);
  clearDirectory(protoChain.protoDir);

  const goModFile = path.join(
    protoChain.repoDir,
    protoChain.chainRepoName,
    "go.mod"
  );
  if (!existsSync(goModFile)) {
    console.error("go.mod not found in the chain repository");
    return;
  }

  collectProtoDirs(
    path.join(protoChain.repoDir, protoChain.chainRepoName),
    [
      {
        in: "proto",
        out: ".",
      },
    ],
    protoChain.protoDir
  );

  const modFile = readFileSync(goModFile, "utf8");
  const lines = modFile.split("\n").map((x) => x.trim());

  for (let i = 0; i < protoChain.protoDependencies.length; i++) {
    const repo = protoChain.protoDependencies[i]!;
    const baseLine = lines.find((x) => x.startsWith(repo.name));

    if (baseLine == null) {
      await checkoutVersion(protoChain, repo, "latest");
    } else {
      const replacementLine = lines.find((x) =>
        x.startsWith(`${repo.name} =>`)
      );

      if (replacementLine != null) {
        console.warn(
          `Found replacement for repo ${repo.name} in go.mod: ${replacementLine}`
        );
        const replacement = replacementLine.split("=>")[1]!.trim();
        const replacementVersion = replacement!.split(" ")[1]!.trim();
        repo.name = replacement.split(" ")[0]!;
        await checkoutVersion(protoChain, repo, replacementVersion);
      } else {
        const version = baseLine.split(" ")[1]!;
        await checkoutVersion(protoChain, repo, version);
      }
    }

    collectProtoDirs(
      path.join(protoChain.repoDir, repo.dirName),
      repo.protoDirs,
      protoChain.protoDir
    );
  }
}

async function clearDirectory(dirPath: string) {
  if (!existsSync(dirPath)) {
    mkdirSync(dirPath);
  }

  rmSync(dirPath, { recursive: true });
  mkdirSync(dirPath);
}

async function checkoutVersion(
  chain: ProtoChain,
  repo: Repo,
  version: string | "latest"
) {
  const repoOutPath = path.join(chain.repoDir, repo.dirName);

  const repoUrl = `https://${repo.name}`;

  const currentVersion = await execAndWait(
    `cd ${repoOutPath} && git tag --points-at HEAD`
  );
  const currentRepoUrl = await execAndWait(
    `cd ${repoOutPath} && git remote get-url origin`
  );

  if (currentVersion == version && repoUrl == currentRepoUrl) {
    console.log(`Found matching version for ${repoUrl} locally`);
    return;
  }
  if (version == "latest" && repoUrl == currentRepoUrl) {
    console.log(`Found clone of ${repoUrl} locally, pulling...`);
    await execAndWait(`cd ${repoOutPath} && git reset --hard && git pull`);
    return;
  }

  if (existsSync(repoOutPath)) {
    rmSync(repoOutPath, { force: true, recursive: true });
  }

  const command =
    version == "latest"
      ? `git clone ${repoUrl} --single-branch ${repoOutPath}`
      : `git clone ${repoUrl} --branch ${version} --single-branch ${repoOutPath}`;

  console.log(command);
  await execAndWait(command);
}

async function execAndWait(command: string) {
  return await new Promise<string>((resolve) =>
    exec(command, (err, stdout, stderr) => {
      resolve(stdout.trim());
    })
  );
}

function collectProtoDirs(
  sourcePath: string,
  protoDirs: ProtoDir[],
  targetPath: string
) {
  protoDirs.forEach((protoDir) => {
    const protoInDir = path.join(sourcePath, protoDir.in);
    const protoOutDir = path.join(targetPath, protoDir.out);
    console.log(`Copying proto source dir ${protoInDir} into ${protoOutDir}`);

    if (!existsSync(protoInDir)) {
      console.error(`Could not find proto dir at ${protoInDir}`);
      process.exit(1);
    }
    if (!existsSync(protoOutDir)) {
      mkdirSync(protoOutDir, {
        recursive: true,
      });
    }

    readdirSync(protoInDir, {
      recursive: false,
      encoding: "utf8",
      withFileTypes: true,
    }).forEach((member) => {
      if (!member.isDirectory() && !member.isFile()) {
        return;
      }
      if (!member.name.endsWith(".proto") && !member.isDirectory()) {
        return;
      }

      cpSync(
        path.join(protoInDir, member.name),
        path.join(protoOutDir, member.name),
        {
          force: true,
          recursive: true,
        }
      );
    });
  });
}

if (process.argv.length != 3) {
  console.error("Usage node update-proto-dir.js [configFilePath]");
  process.exit(1);
}

if (!existsSync(process.argv[2]!)) {
  console.error(`Config file not found at ${process.argv[2]}`);
  process.exit(1);
}

main(process.argv[2]!);
