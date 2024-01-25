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
    if ((0, node_fs_1.existsSync)(repoOutPath)) {
        if (!(await tryCheckoutVersionLocally(repoUrl, repoOutPath, version))) {
            (0, node_fs_1.rmSync)(repoOutPath, { force: true, recursive: true });
        }
        else {
            return;
        }
    }
    const command = version == "latest"
        ? `git clone ${repoUrl} --single-branch ${repoOutPath}`
        : `git clone ${repoUrl} --branch ${version} --single-branch ${repoOutPath}`;
    console.log(command);
    const res = await execAndWait(command);
    if (res.err.includes(`not found in upstream origin`)) {
        const versionParts = version.split("-");
        if (versionParts.length != 3 ||
            (versionParts[2] != null && versionParts[2].length != 12)) {
            console.error(`Branch ${version} could not be found in repository and parsing it as a commit hash failed!`);
            process.exit(1);
        }
        const commitHash = versionParts[2];
        console.log(`${version} appears to be a commit hash instead of a branch. Cloning and checking out commit.`);
        const command2 = `git clone ${repoUrl} ${repoOutPath} && cd ${repoOutPath} && git reset --hard ${commitHash}`;
        console.log(command2);
        const resetRes = await execAndWait(command2);
        if (!resetRes.std.includes("HEAD is now at")) {
            console.error(resetRes.err);
            console.error(`Failed to check out commit hash ${commitHash}. Please manually resolve dependency ${repoUrl}`);
            process.exit(1);
        }
    }
}
async function tryCheckoutVersionLocally(repoUrl, repoPath, version) {
    const currentVersion = await execAndWait(`cd ${repoPath} && git tag --points-at HEAD`).then((x) => x.std);
    const currentRepoUrl = await execAndWait(`cd ${repoPath} && git remote get-url origin`).then((x) => x.std);
    if (currentVersion == version && repoUrl == currentRepoUrl) {
        console.log(`Found matching version for ${repoUrl} locally`);
        return true;
    }
    if (version == "latest" && repoUrl == currentRepoUrl) {
        console.log(`Found clone of ${repoUrl} locally, pulling...`);
        await execAndWait(`cd ${repoPath} && git reset --hard && git pull`);
        return true;
    }
    const versionParts = version.split("-");
    if (versionParts.length != 3 ||
        (versionParts[2] != null && versionParts[2].length != 12)) {
        return false;
    }
    const commitHash = versionParts[2];
    const commitCheckoutResp = await execAndWait(`cd ${repoPath} && git reset --hard ${commitHash}`);
    if (commitCheckoutResp.std.includes("HEAD is now at")) {
        console.log(`Found matching version for ${repoUrl} locally`);
        return true;
    }
    return false;
}
async function execAndWait(command) {
    return await new Promise((resolve) => (0, node_child_process_1.exec)(command, (err, stdout, stderr) => {
        resolve({
            std: stdout.trim(),
            err: stderr.trim(),
        });
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