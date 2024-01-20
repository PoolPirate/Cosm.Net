"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const node_fs_1 = require("node:fs");
const node_child_process_1 = require("node:child_process");
const node_path_1 = __importDefault(require("node:path"));
const repos_1 = require("./repos");
function validateProtoChain(protoChain) {
    if (!(0, node_fs_1.existsSync)(node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoName))) {
        console.error(`Given chain repo does not exist at ${node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoName)}`);
        return null;
    }
    if (!(0, node_fs_1.existsSync)(protoChain.protoDir)) {
        console.error("Given protoDir does not exist");
        return null;
    }
}
async function main(configPath) {
    const protoChain = (0, repos_1.loadProtoChainFromFile)(configPath);
    validateProtoChain(protoChain);
    console.log(`Clearing proto directory at ${protoChain.protoDir}`);
    clearDirectory(protoChain.protoDir);
    const goModFile = node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoName, "go.mod");
    if (!(0, node_fs_1.existsSync)(goModFile)) {
        console.error("go.mod not found in the chain repository");
        return;
    }
    collectProtoDirs(node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoName), [
        {
            in: "proto",
            out: ".",
        },
    ], protoChain.protoDir);
    const modFile = (0, node_fs_1.readFileSync)(goModFile, "utf8");
    const lines = modFile.split("\n").map((x) => x.trim());
    for (let i = 0; i < protoChain.protoDependencies.length; i++) {
        const repo = protoChain.protoDependencies[i];
        const baseLine = lines.find((x) => x.startsWith(repo.name));
        if (baseLine == null) {
            await checkoutVersion(protoChain, repo, "latest");
        }
        else {
            const replacementLine = lines.find((x) => x.startsWith(`${repo.name} =>`));
            if (replacementLine != null) {
                console.warn(`Found replacement for repo ${repo.name} in go.mod: ${replacementLine}`);
                const replacement = replacementLine.split("=>")[1].trim();
                const replacementVersion = replacement.split(" ")[1].trim();
                repo.name = replacement.split(" ")[0];
                await checkoutVersion(protoChain, repo, replacementVersion);
            }
            else {
                const version = baseLine.split(" ")[1];
                await checkoutVersion(protoChain, repo, version);
            }
        }
        collectProtoDirs(node_path_1.default.join(protoChain.repoDir, repo.dirName), repo.protoDirs, protoChain.protoDir);
    }
}
async function clearDirectory(dirPath) {
    if (!(0, node_fs_1.existsSync)(dirPath)) {
        (0, node_fs_1.mkdirSync)(dirPath);
    }
    (0, node_fs_1.rmSync)(dirPath, { recursive: true });
    (0, node_fs_1.mkdirSync)(dirPath);
}
async function checkoutVersion(chain, repo, version) {
    const repoOutPath = node_path_1.default.join(chain.repoDir, repo.dirName);
    const repoUrl = `https://${repo.name}`;
    const currentVersion = await execAndWait(`cd ${repoOutPath} && git tag --points-at HEAD`);
    const currentRepoUrl = await execAndWait(`cd ${repoOutPath} && git remote get-url origin`);
    if (currentVersion == version && repoUrl == currentRepoUrl) {
        console.log(`Found matching version for ${repoUrl} locally`);
        return;
    }
    if (version == "latest" && repoUrl == currentRepoUrl) {
        console.log(`Found clone of ${repoUrl} locally, pulling...`);
        await execAndWait(`cd ${repoOutPath} && git reset --hard && git pull`);
        return;
    }
    if ((0, node_fs_1.existsSync)(repoOutPath)) {
        (0, node_fs_1.rmSync)(repoOutPath, { force: true, recursive: true });
    }
    const command = version == "latest"
        ? `git clone ${repoUrl} --single-branch ${repoOutPath}`
        : `git clone ${repoUrl} --branch ${version} --single-branch ${repoOutPath}`;
    console.log(command);
    await execAndWait(command);
}
async function execAndWait(command) {
    return await new Promise((resolve) => (0, node_child_process_1.exec)(command, (err, stdout, stderr) => {
        resolve(stdout.trim());
    }));
}
function collectProtoDirs(sourcePath, protoDirs, targetPath) {
    protoDirs.forEach((protoDir) => {
        const protoInDir = node_path_1.default.join(sourcePath, protoDir.in);
        const protoOutDir = node_path_1.default.join(targetPath, protoDir.out);
        console.log(`Copying proto source dir ${protoInDir} into ${protoOutDir}`);
        if (!(0, node_fs_1.existsSync)(protoInDir)) {
            console.error(`Could not find proto dir at ${protoInDir}`);
            process.exit(1);
        }
        if (!(0, node_fs_1.existsSync)(protoOutDir)) {
            (0, node_fs_1.mkdirSync)(protoOutDir, {
                recursive: true,
            });
        }
        (0, node_fs_1.readdirSync)(protoInDir, {
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
            (0, node_fs_1.cpSync)(node_path_1.default.join(protoInDir, member.name), node_path_1.default.join(protoOutDir, member.name), {
                force: true,
                recursive: true,
            });
        });
    });
}
if (process.argv.length != 3) {
    console.error("Usage node update-proto-dir.js [configFilePath]");
    process.exit(1);
}
if (!(0, node_fs_1.existsSync)(process.argv[2])) {
    console.error(`Config file not found at ${process.argv[2]}`);
    process.exit(1);
}
main(process.argv[2]);
//# sourceMappingURL=update-proto-dir.js.map