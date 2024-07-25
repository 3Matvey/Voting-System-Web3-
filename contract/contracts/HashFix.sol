// SPDX-License-Identifier: MIT
pragma solidity ^0.8.0;

library HashFix{
    
    function hashStruct (string memory field1, string memory field2) internal pure returns (bytes32){
        return keccak256(abi.encodePacked("field1:", field1, ";field2:", field2));
    }

    function hashStruct(bytes[] memory fields) internal pure returns (bytes32) {
        return keccak256(abi.encode(fields));
    }
}
