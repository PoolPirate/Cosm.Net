import {
  readFileSync,
  existsSync,
  cpSync,
  readdirSync,
  rmSync,
  mkdirSync,
  rmdirSync,
  rm,
} from "node:fs";
import { exec } from "node:child_process";
import path from "node:path";
import { ProtoChain, ProtoDir, Repo, loadProtoChainFromFile } from "./repos";

async function main(configPath: string) {
  console.log("Running proto sync...");
  const protoChain = loadProtoChainFromFile(configPath);
  validateProtoChain(protoChain);
  console.log(`Clearing proto directory at ${protoChain.protoDir}`);
  clearDirectory(protoChain.protoDir);

  if (protoChain.chainRepoName !== null) {
    console.log(`Checking out chain repo commit ${protoChain.commitHash}`);
    const checkoutSuccess = await tryCheckoutCommitHash(
      `https://${protoChain.chainRepoName}`,
      path.join(
        protoChain.repoDir,
        protoChain.chainRepoName.split("/").at(-1)!
      ),
      protoChain.commitHash
    );

    if (!checkoutSuccess) {
      process.exit(1);
    }
  } else {
    console.log("Chain repo checkout disabled");
  }

  const goModFile = path.join(
    protoChain.repoDir,
    protoChain.chainRepoPath,
    protoChain.goModPath ?? ".",
    "go.mod"
  );

  if (!existsSync(goModFile)) {
    console.error("go.mod not found in the chain repository");
    process.exit(1);
  }

  if (!protoChain.hasNoProtos) {
    collectProtoDirs(
      path.join(protoChain.repoDir, protoChain.chainRepoPath),
      [
        {
          in: "proto",
          out: ".",
        },
      ],
      protoChain.protoDir
    );
  }

  const modFile = readFileSync(goModFile, "utf8");
  const lines = modFile
    .split("\n")
    .map((x) => x.trim())
    .filter((x) => !x.startsWith("//"));

  for (let i = 0; i < protoChain.protoDependencies.length; i++) {
    const repo = protoChain.protoDependencies[i]!;
    const baseLines = lines.filter(
      (x) => x.startsWith(`${repo.name} `) || x.startsWith(`${repo.name}/`)
    );

    if (baseLines.length == 0 && !repo.isExternal) {
      console.error(`Failed to find dependency ${repo.name} in go.mod file`);
      process.exit(1);
    }

    const baseLine = selectValidVersion(repo, baseLines);

    if (repo.forceExternal && !repo.isExternal) {
      console.error(
        `Dependency ${repo.name} marked as forceExternal but is not marked as external`
      );
    }
    if (baseLine != null && repo.isExternal && !repo.forceExternal) {
      console.error(
        `Found dependency ${repo.name} in go.mod file, but expected it to be an external dependency`
      );
      process.exit(1);
    }

    if (repo.forceExternal) {
      await checkoutVersion(protoChain, repo, repo.externalVersion ?? "latest");
    } else if (baseLine == null) {
      await checkoutVersion(protoChain, repo, "latest");
    } else {
      let replacementLines = lines.filter(
        (x) =>
          (x.startsWith(`${repo.name}`) && x.includes("=>")) ||
          x.includes(`${repo.name} =>`)
      );

      if (replacementLines.length > 0) 
      {
        const filteredLines = replacementLines.filter(x => repo.branchNameExcludes == undefined || !x.includes(repo.branchNameExcludes));
        if (filteredLines.length == 0) 
        {
          console.error(`None of the replacements for ${repo.name} don't contain forbidden content ${repo.branchNameExcludes}`);
          process.exit(1);
        }

        replacementLines = filteredLines;
      }

      if (replacementLines.length > 1)
      {
        console.error(`Found more than one replacement for ${repo.name}, consider adding "branchNameExcludes"!`);
        process.exit(1);
      }

      const replacementLine = replacementLines.length == 1
        ? replacementLines[0]
        : null;

      if (replacementLine != null) {
        console.warn(
          `Found replacement for repo ${repo.name} in go.mod: ${replacementLine}`
        );
        const replacement = replacementLine.split("=>")[1]!.trim();
        const versionInfo = parseVersion(repo, replacement);

        repo.name = versionInfo.repoUrl;
        await checkoutVersion(protoChain, repo, versionInfo.version);
      } else {
        const versionInfo = parseVersion(repo, baseLine);
        await checkoutVersion(protoChain, repo, versionInfo.version);
      }
    }

    collectProtoDirs(
      path.join(protoChain.repoDir, repo.dirName),
      repo.protoDirs,
      protoChain.protoDir
    );
  }

  console.info("Proto sync completed");
}

function validateProtoChain(protoChain: ProtoChain) {
  if (!existsSync(path.join(protoChain.repoDir, protoChain.chainRepoPath))) {
    console.error(
      `Given chain repo does not exist at ${path.join(
        protoChain.repoDir,
        protoChain.chainRepoPath
      )}`
    );
    return null;
  }
  if (!existsSync(protoChain.protoDir)) {
    console.error("Given protoDir does not exist");
    return null;
  }
}

function selectValidVersion(repo: Repo, options: string[]): string | null {
  if (options.length == 0) {
    return null;
  }

  if (options.length == 1) {
    return options[0]!;
  }

  const replacementOptions = options.filter((x) => x.includes("=>"));

  if (replacementOptions.length == 1) {
    return replacementOptions[0]!;
  } else if (replacementOptions.length > 1) {
    return replacementOptions.sort((a, b) => b.length - a.length)[0]!;
  }

  const slashcountedOptions = options.map((option) => {
    return {
      value: option,
      slashCount: option.match(/\//g)?.length ?? 0,
    };
  });

  return slashcountedOptions.sort((a, b) => a.slashCount - b.slashCount)[0]!
    .value;
}

export type RepoVersion = {
  repoUrl: string;
  version: string;
};

function parseVersion(repo: Repo, godModRepo: string) {
  const actualRepoName = godModRepo.split(" ")[0]!.trim();
  const actualVersion = godModRepo.split(" ")[1]!.trim();

  const repoNameParts = repo.name.split("/");
  const actualRepoNameParts = actualRepoName.split("/");

  const repoNameSuffix = repoNameParts.slice(-1)[0]!.trim();
  const actualRepoNameSuffix = actualRepoNameParts.slice(-1)[0]!.trim();

  if (
    repoNameSuffix == actualRepoNameSuffix ||
    repoNameParts.length == actualRepoNameParts.length ||
    actualRepoNameSuffix[0] == "v"
  ) {
    return {
      repoUrl: actualRepoNameParts.slice(0, repoNameParts.length).join("/"),
      version: actualVersion,
    } satisfies RepoVersion;
  }

  return {
    repoUrl: actualRepoNameParts.slice(0, repoNameParts.length).join("/"),
    version: `${actualRepoNameSuffix}/${actualVersion}`,
  } satisfies RepoVersion;
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

  if (existsSync(repoOutPath)) {
    if (!(await tryCheckoutVersionLocally(repoUrl, repoOutPath, version))) {
      rmSync(repoOutPath, { force: true, recursive: true });
    } else {
      return;
    }
  }

  const command =
    version == "latest"
      ? `git clone ${repoUrl} --single-branch ${repoOutPath}`
      : `git clone ${repoUrl} --branch ${version} --single-branch ${repoOutPath}`;

  console.log(command);
  const res = await execAndWait(command);

  if (res.err.includes(`not found in upstream origin`)) {
    const versionParts = version.split("-");

    if (
      versionParts.length != 3 ||
      (versionParts[2] != null && versionParts[2].length != 12)
    ) {
      console.error(
        `Branch ${version} could not be found in repository and parsing it as a commit hash failed!`
      );
      process.exit(1);
    }

    const commitHash = versionParts[2];
    console.log(
      `${version} appears to be a commit hash instead of a branch. Cloning and checking out commit.`
    );

    const command2 = `git clone ${repoUrl} ${repoOutPath} && cd ${repoOutPath} && git reset --hard ${commitHash}`;
    console.log(command2);
    const resetRes = await execAndWait(command2);

    if (!resetRes.std.includes("HEAD is now at")) {
      console.error(resetRes.err);
      console.error(
        `Failed to check out commit hash ${commitHash}. Please manually resolve dependency ${repoUrl}`
      );
      process.exit(1);
    }
  }
}

async function tryCheckoutVersionLocally(
  repoUrl: string,
  repoPath: string,
  version: string
) {
  const currentVersion = await execAndWait(
    `cd ${repoPath} && git tag --points-at HEAD`
  ).then((x) => x.std);
  const currentRepoUrl = await execAndWait(
    `cd ${repoPath} && git remote get-url origin`
  ).then((x) => x.std);

  if (currentVersion == version && repoUrl == currentRepoUrl) {
    console.log(`Found matching version for ${repoUrl} locally`);
    return true;
  }
  if (version == "latest" && repoUrl == currentRepoUrl) {
    console.log(`Found clone of ${repoUrl} locally. No specific version requirement configured, skipping...`);
    return true;
  }

  const versionParts = version.split("-");
  if (
    versionParts.length != 3 ||
    (versionParts[2] != null && versionParts[2].length != 12)
  ) {
    return false;
  }
  const commitHash = versionParts[2];

  const commitCheckoutResp = await execAndWait(
    `cd ${repoPath} && git reset --hard ${commitHash}`
  );

  if (commitCheckoutResp.std.includes("HEAD is now at")) {
    console.log(`Found matching version for ${repoUrl} locally`);
    return true;
  }

  return false;
}

async function tryCheckoutCommitHash(
  repoUrl: string,
  repoDirectory: string,
  commitHash: string
) {
  if (!existsSync(repoDirectory)) {
    const cloneCmd = `git clone ${repoUrl} ${repoDirectory}`;
    console.log(cloneCmd);
    await execAndWait(cloneCmd);
  }

  let response = await execAndWait(
    `cd ${repoDirectory} && git checkout ${commitHash}`
  );

  if (
    response.err.includes("not a git repository") ||
    response.err.includes("unable to read tree")
  ) {
    rmSync(repoDirectory, {
      force: true,
      recursive: true,
      maxRetries: 5,
    });

    const cloneCmd = `git clone ${repoUrl} ${repoDirectory}`;
    console.log(cloneCmd);
    await execAndWait(cloneCmd);
    response = await execAndWait(
      `cd ${repoDirectory} && git checkout ${commitHash}`
    );
  }

  if (response.err.includes("reference is not a tree")) {
    const pullCommand = `cd ${repoDirectory} && git pull`;
    console.log(pullCommand);
    await execAndWait(pullCommand);
    response = await execAndWait(
      `cd ${repoDirectory} && git checkout ${commitHash}`
    );
  }

  if (!response.err.includes("HEAD is now at")) {
    console.log(`Git output: ${response.err}`);
    console.error(`Checking out ${commitHash} in ${repoDirectory} failed`);
    return false;
  }

  return true;
}

async function execAndWait(command: string) {
  return await new Promise<{ std: string; err: string }>((resolve) =>
    exec(command, (err, stdout, stderr) => {
      resolve({
        std: stdout.trim(),
        err: stderr.trim(),
      });
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
