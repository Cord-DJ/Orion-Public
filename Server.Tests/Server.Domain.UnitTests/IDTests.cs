// using System.Collections.Generic;
// using Cord;

// namespace Server.Domain.UnitTests;

// public class IDTests {
//     public static IEnumerable<TestCaseData> RandomIDs_Success() {
//         yield return new TestCaseData(420UL, "11/29/2021 00:00:00 +00:00");
//         yield return new TestCaseData(36237147753152512UL, "03/08/2022 23:53:29 +00:00");
//     }

//     [Test]
//     [TestCaseSource(nameof(RandomIDs_Success))]
//     public void HasIDValidTime(ulong id, string date) {
//         var obj = new ID(id);
//         Assert.AreEqual(date, obj.CreatedAt.ToString());
//     }

//     [Test]
//     public void ParseValidation() {
//         var id1 = new ID(36237147753152512UL);
//         var id2 = new ID(36237147753152512UL);
//         var id3 = new ID(36237147753152510UL);
//         var id4 = new ID(420_000_000_690);

//         Assert.AreEqual(id1, id2);
//         Assert.AreNotEqual(id1, id3);
//         Assert.AreNotEqual(id3, id4);
//         Assert.AreNotEqual(id1, id4);
//     }
// }
