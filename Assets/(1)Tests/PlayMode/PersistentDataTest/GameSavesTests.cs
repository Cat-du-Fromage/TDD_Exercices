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
            if(Directory.Exists(MainSaveDirectory.Instance.MainSaveDirInfo.FullName))
                MainSaveDirectory.Instance.MainSaveDirInfo.Delete(true);
            //DirectoryInfo path = MainSaveDirectory.GameSavesPath;
        }
        
        [Test, Order(1)]
        public void MainSaveDirectory_MainSaveFolderExist_True()
        {
            //Arrange
            
            //Act
            bool checkExist = MainSaveDirectory.Instance.Exist();
            //Assert
            Assert.IsTrue(checkExist);
        }
        
        [Test, Order(2)]
        public void MainSaveDirectory_GetNumSavesInside_Zero()
        {
            //Arrange
            
            //Act
            int getNumSaves = MainSaveDirectory.Instance.GetNumSubfolders();
            //Assert
            Assert.Zero(getNumSaves);
        }
        
        //This stand true even if the Main Root doesn't exist 
        //because DirectoryInfo.CreateSubdirectory() Automatically create the Root Directory
        // see : https://docs.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.createsubdirectory?view=net-5.0
        [Test, Order(3)]
        public void MainSaveDirectory_GetNumSavesInside_2()
        {
            //Arrange
            int expected = 2;
            string path1 = "SaveTest1";
            string path2 = "SaveTest2";
            
            //Act
            MainSaveDirectory.Instance.MainSaveDirInfo.CreateSubdirectory(path1);
            MainSaveDirectory.Instance.MainSaveDirInfo.CreateSubdirectory(path2);
            int getNumSaves = MainSaveDirectory.Instance.GetNumSubfolders();
            //Assert
            Assert.AreEqual(expected, getNumSaves);
        }
    }
}
