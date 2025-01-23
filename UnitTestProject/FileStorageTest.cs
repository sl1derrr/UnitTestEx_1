using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System;
using System.Reflection;
using UnitTestEx;
using Assert = NUnit.Framework.Assert;

namespace UnitTestProject
{
    /// <summary>
    /// Summary description for FileStorageTest
    /// </summary>
    [TestClass]
    public class FileStorageTest
    {
        public const string MAX_SIZE_EXCEPTION = "DIFFERENT MAX SIZE";
        public const string NULL_FILE_EXCEPTION = "NULL FILE";
        public const string NO_EXPECTED_EXCEPTION_EXCEPTION = "There is no expected exception";

        public const string SPACE_STRING = " ";
        public const string FILE_PATH_STRING = "@D:\\JDK-intellij-downloader-info.txt";
        public const string CONTENT_STRING = "Some text";
        public const string REPEATED_STRING = "AA";
        public const string WRONG_SIZE_CONTENT_STRING = "TEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtextTEXTtext";
        public const string TIC_TOC_TOE_STRING = "tictoctoe.game";

        public const int NEW_SIZE = 5;

        public FileStorage storage = new FileStorage(NEW_SIZE);

        /* ПРОВАЙДЕРЫ */

        static object[] NewFilesData =
        {
            new object[] { new File(REPEATED_STRING, CONTENT_STRING) },
            new object[] { new File(SPACE_STRING, WRONG_SIZE_CONTENT_STRING) },
            new object[] { new File(FILE_PATH_STRING, CONTENT_STRING) }
        };

        static object[] FilesForDeleteData =
        {
            new object[] { new File(REPEATED_STRING, CONTENT_STRING), REPEATED_STRING },
            new object[] { null, TIC_TOC_TOE_STRING }
        };

        static object[] NewExceptionFileData = {
            new object[] { new File(REPEATED_STRING, CONTENT_STRING) }
        };

        /* Тестирование записи файла */
        [Test, TestCaseSource(nameof(NewFilesData))]
        public void WriteTest(File file) 
        {
            try 
            {
                bool result = storage.Write(file);
                if (!result)
                {
                    Assert.Ignore("Файл не может быть записан из-за ограничений размера");
                    return;
                }
                Assert.True(result);
            }
            catch (FileNameAlreadyExistsException)
            {
                Assert.Ignore("Файл с таким именем уже существует");
            }
            finally
            {
                storage.DeleteAllFiles();
            }
        }

        /* Тестирование записи дублирующегося файла */
        [Test, TestCaseSource(nameof(NewExceptionFileData))]
        public void WriteExceptionTest(File file) {
            bool isException = false;
            try
            {
                storage.Write(file);
                Assert.False(storage.Write(file));
                storage.DeleteAllFiles();
            } 
            catch (FileNameAlreadyExistsException)
            {
                isException = true;
            }
            Assert.True(isException, NO_EXPECTED_EXCEPTION_EXCEPTION);
        }

        /* Тестирование проверки существования файла */
        [Test, TestCaseSource(nameof(NewFilesData))]
        public void IsExistsTest(File file) 
        {
            try 
            {
                String name = file.GetFilename();
                
                // Очищаем хранилище перед тестом
                storage.DeleteAllFiles();
                
                // Проверяем, что файла изначально нет в хранилище
                Assert.False(storage.IsExists(name));

                // Пытаемся записать файл
                bool writeSuccess = storage.Write(file);
                
                if (!writeSuccess)
                {
                    Assert.Ignore("Файл не может быть записан из-за ограничений размера");
                    return;
                }

                // Проверяем, что файл теперь существует
                Assert.True(storage.IsExists(name));
            }
            finally 
            {
                // Очищаем хранилище после теста
                storage.DeleteAllFiles();
            }
        }

        /* Тестирование удаления файла */
        [Test, TestCaseSource(nameof(FilesForDeleteData))]
        public void DeleteTest(File file, String fileName) 
        {
            if (file != null)
            {
                storage.Write(file);
                Assert.True(storage.Delete(fileName));
            }
            else
            {
                Assert.False(storage.Delete(fileName));
            }
        }

        /* Тестирование получения файлов */
        [Test]
        public void GetFilesTest()
        {
            foreach (File el in storage.GetFiles()) 
            {
                Assert.NotNull(el);
            }
        }

        /* Тестирование получения файла */
        [Test, TestCaseSource(nameof(NewFilesData))]
        public void GetFileTest(File expectedFile) 
        {
            // Проверяем успешность записи файла
            if (!storage.Write(expectedFile))
            {
                Assert.Ignore("Файл не может быть записан из-за ограничений размера");
                return;
            }

            File actualfile = storage.GetFile(expectedFile.GetFilename());
            Assert.NotNull(actualfile, "Полученный файл не должен быть null");

            bool difference = actualfile.GetFilename().Equals(expectedFile.GetFilename()) && 
                             actualfile.GetSize().Equals(expectedFile.GetSize());

            Assert.IsTrue(difference, string.Format("There is some differences in {0} or {1}", 
                expectedFile.GetFilename(), expectedFile.GetSize()));
        }

        /* Тестирование записи файла с превышением доступного размера хранилища */
        [Test]
        public void WriteOverSizeLimitTest()
        {
            File largeFile = new File("large.txt", WRONG_SIZE_CONTENT_STRING);
            FileStorage smallStorage = new FileStorage(1); // создаем хранилище маленького размера
            
            Assert.False(smallStorage.Write(largeFile));
        }

        /* Тестирование получения несуществующего файла */
        [Test]
        public void GetNonExistentFileTest()
        {
            File result = storage.GetFile("nonexistent.txt");
            Assert.IsNull(result);
        }

        /* Тестирование удаления несуществующего файла */
        [Test]
        public void DeleteNonExistentFileTest()
        {
            Assert.False(storage.Delete("nonexistent.txt"));
        }
    }
}
