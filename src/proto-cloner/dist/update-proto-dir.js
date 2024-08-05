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
    if (!(0, node_fs_1.existsSync)(node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoPath))) {
        console.error(`Given chain repo does not exist at ${node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoPath)}`);
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
    if (protoChain.chainRepoName !== null) {
        console.log(`Checking out chain repo commit ${protoChain.commitHash}`);
        const checkoutSuccess = await tryCheckoutCommitHash(`https://${protoChain.chainRepoName}`, node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoName.split("/").at(-1)), protoChain.commitHash);
        if (!checkoutSuccess) {
            process.exit(1);
        }
    }
    else {
        console.log("Chain repo checkout disabled");
    }
    const goModFile = node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoPath, protoChain.goModPath ?? ".", "go.mod");
    if (!(0, node_fs_1.existsSync)(goModFile)) {
        console.error("go.mod not found in the chain repository");
        process.exit(1);
    }
    if (!protoChain.hasNoProtos) {
        collectProtoDirs(node_path_1.default.join(protoChain.repoDir, protoChain.chainRepoPath), [
            {
                in: "proto",
                out: ".",
            },
        ], protoChain.protoDir);
    }
    const modFile = (0, node_fs_1.readFileSync)(goModFile, "utf8");
    const lines = modFile
        .split("\n")
        .map((x) => x.trim())
        .filter((x) => !x.startsWith("//"));
    for (let i = 0; i < protoChain.protoDependencies.length; i++) {
        const repo = protoChain.protoDependencies[i];
        const baseLines = lines.filter((x) => x.startsWith(`${repo.name} `) || x.startsWith(`${repo.name}/`));
        if (baseLines.length == 0 && !repo.isExternal) {
            console.error(`Failed to find dependency ${repo.name} in go.mod file`);
            process.exit(1);
        }
        const baseLine = selectValidVersion(repo, baseLines);
        if (repo.forceExternal && !repo.isExternal) {
            console.error(`Dependency ${repo.name} marked as forceExternal but is not marked as external`);
        }
        if (baseLine != null && repo.isExternal && !repo.forceExternal) {
            console.error(`Found dependency ${repo.name} in go.mod file, but expected it to be an external dependency`);
            process.exit(1);
        }
        if (repo.forceExternal) {
            await checkoutVersion(protoChain, repo, repo.externalVersion ?? "latest");
        }
        else if (baseLine == null) {
            await checkoutVersion(protoChain, repo, "latest");
        }
        else {
            const replacementLine = lines.find((x) => (x.startsWith(`${repo.name}`) && x.includes("=>")) ||
                x.includes(`${repo.name} =>`));
            if (replacementLine != null) {
                console.warn(`Found replacement for repo ${repo.name} in go.mod: ${replacementLine}`);
                const replacement = replacementLine.split("=>")[1].trim();
                const versionInfo = parseVersion(repo, replacement);
                repo.name = versionInfo.repoUrl;
                await checkoutVersion(protoChain, repo, versionInfo.version);
            }
            else {
                const versionInfo = parseVersion(repo, baseLine);
                await checkoutVersion(protoChain, repo, versionInfo.version);
            }
        }
        collectProtoDirs(node_path_1.default.join(protoChain.repoDir, repo.dirName), repo.protoDirs, protoChain.protoDir);
    }
    console.info("Proto sync completed");
}
function selectValidVersion(repo, options) {
    if (options.length == 0) {
        return null;
    }
    if (options.length == 1) {
        return options[0];
    }
    const replacementOptions = options.filter((x) => x.includes("=>"));
    if (replacementOptions.length == 1) {
        return replacementOptions[0];
    }
    else if (replacementOptions.length > 1) {
        return replacementOptions.sort((a, b) => b.length - a.length)[0];
    }
    const slashcountedOptions = options.map((option) => {
        return {
            value: option,
            slashCount: option.match(/\//g)?.length ?? 0,
        };
    });
    return slashcountedOptions.sort((a, b) => a.slashCount - b.slashCount)[0]
        .value;
}
function parseVersion(repo, godModRepo) {
    const actualRepoName = godModRepo.split(" ")[0].trim();
    const actualVersion = godModRepo.split(" ")[1].trim();
    const repoNameParts = repo.name.split("/");
    const actualRepoNameParts = actualRepoName.split("/");
    const repoNameSuffix = repoNameParts.slice(-1)[0].trim();
    const actualRepoNameSuffix = actualRepoNameParts.slice(-1)[0].trim();
    if (repoNameSuffix == actualRepoNameSuffix ||
        repoNameParts.length == actualRepoNameParts.length ||
        actualRepoNameSuffix[0] == "v") {
        return {
            repoUrl: actualRepoNameParts.slice(0, repoNameParts.length).join("/"),
            version: actualVersion,
        };
    }
    return {
        repoUrl: actualRepoNameParts.slice(0, repoNameParts.length).join("/"),
        version: `${actualRepoNameSuffix}/${actualVersion}`,
    };
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
async function tryCheckoutCommitHash(repoUrl, repoDirectory, commitHash) {
    if (!(0, node_fs_1.existsSync)(repoDirectory)) {
        const cloneCmd = `git clone ${repoUrl} ${repoDirectory}`;
        console.log(cloneCmd);
        await execAndWait(cloneCmd);
    }
    let response = await execAndWait(`cd ${repoDirectory} && git checkout ${commitHash}`);
    if (response.err.includes("not a git repository")) {
        (0, node_fs_1.rmSync)(repoDirectory, {
            force: true,
            recursive: true,
            maxRetries: 5,
        });
        const cloneCmd = `git clone ${repoUrl} ${repoDirectory}`;
        console.log(cloneCmd);
        await execAndWait(cloneCmd);
        response = await execAndWait(`cd ${repoDirectory} && git checkout ${commitHash}`);
    }
    if (response.err.includes("reference is not a tree")) {
        const pullCommand = `cd ${repoDirectory} && git pull`;
        console.log(pullCommand);
        await execAndWait(pullCommand);
        response = await execAndWait(`cd ${repoDirectory} && git checkout ${commitHash}`);
    }
    if (!response.err.includes("HEAD is now at")) {
        console.log(response.err);
        console.error(`Checking out ${commitHash} in ${repoDirectory} failed`);
        return false;
    }
    return true;
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