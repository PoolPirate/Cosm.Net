"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.loadProtoChainFromFile = void 0;
const node_fs_1 = require("node:fs");
const node_process_1 = require("node:process");
function loadProtoChainFromFile(path) {
    try {
        const content = (0, node_fs_1.readFileSync)(path, "utf8").trim();
        return JSON.parse(content);
    }
    catch (error) {
        console.error(`Failed reading config at ${path}`);
        (0, node_process_1.exit)(1);
    }
}
exports.loadProtoChainFromFile = loadProtoChainFromFile;
//# sourceMappingURL=repos.js.map