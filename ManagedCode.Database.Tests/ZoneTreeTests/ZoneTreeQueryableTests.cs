﻿// using System;
// using System.Threading.Tasks;
// using FluentAssertions;
// using ManagedCode.Database.Tests.BaseTests;
// using ManagedCode.Database.Tests.Common;
// using ManagedCode.Database.Tests.TestContainers;
//
// namespace ManagedCode.Database.Tests.ZoneTreeTests;
//
// public class ZoneTreeQueryableTests : BaseQueryableTests<string, TestZoneTreeItem>
// {
//     public ZoneTreeQueryableTests() : base(new ZoneTreeTestContainer())
//     {
//     }
//
//     public override async Task ThenByDescending_TakeNull_ReturnException()
//     {
//         // Arrange
//         int itemsCountToInsert = 1;
//
//         await CreateAndInsertItemsAsync(itemsCountToInsert);
//
//         // Act
//         var itemsResult = () => Collection.Query
//             .OrderBy(o => o.StringData)
//             .ThenByDescending(null)
//             .ToListAsync();
//
//         // Assert
//         await itemsResult
//             .Should()
//             .ThrowAsync<NullReferenceException>();
//     }
//     
//     public override async Task ThenBy_TakeNull_ReturnException()
//     {
//         // Arrange
//         int itemsCountToInsert = 1;
//
//         await CreateAndInsertItemsAsync(itemsCountToInsert);
//
//         // Act
//         var itemsResult = () => Collection.Query
//             .OrderBy(o => o.StringData)
//             .ThenBy(null)
//             .ToListAsync();
//
//         // Assert
//         await itemsResult
//             .Should()
//             .ThrowAsync<NullReferenceException>();
//     }
// }