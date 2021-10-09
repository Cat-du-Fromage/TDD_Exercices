using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaizerWaldCode.PersistentData;
using NUnit.Framework;
using UnityEngine;

namespace PlayModeTest
{
    [TestFixture]
    public class GameSavesTests
    {
        [SetUp]
        public void SetUp()
        {
            DirectoryInfo path = SaveDirectory.GameSavesPath;
        }
        
        [Test]
        public void SaveDirectory_SaveGameExist_True()
        {
            //Arrange
            
            //Act
            
            //Assert
            DirectoryAssert.Exists(SaveDirectory.GameSavesPath);
        }
    }
}
