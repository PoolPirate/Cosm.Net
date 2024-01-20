"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.loadProtoChainFromFile = void 0;
const node_fs_1 = require("node:fs");
function loadProtoChainFromFile(path) {
    const content = (0, node_fs_1.readFileSync)(path, "utf8").trim();
    return JSON.parse(content);
}
exports.loadProtoChainFromFile = loadProtoChainFromFile;
//# sourceMappingURL=repos.js.map