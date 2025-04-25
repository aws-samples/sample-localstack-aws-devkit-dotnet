// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

namespace OrderManagementSystem.Common.Interfaces;

public interface IFileStorageService
{
    Task SaveFile(string filePathAndName, string fileContent);
}
