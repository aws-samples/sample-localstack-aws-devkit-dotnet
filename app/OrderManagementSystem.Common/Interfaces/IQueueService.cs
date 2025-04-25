// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

namespace OrderManagementSystem.Common.Interfaces;

public interface IQueueService
{
    Task<string?> PublishMessage<T>(T message);
    Task<List<T>?> ReceiveMessages<T>(int count);
}
