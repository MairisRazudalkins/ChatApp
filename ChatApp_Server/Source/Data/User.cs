using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using User;

namespace Server
{
    [Serializable]
    public class User
    {
        private static readonly string usersPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Data\\Users\\";

        private static readonly byte[] key = File.ReadAllBytes(usersPath + "key.txt"), iv = File.ReadAllBytes(usersPath + "iv.txt");

        public LoginDetails loginDetails;
        public UserInfo info;

        private User() { }

        public User(LoginDetails loginDetails)
        {
            this.loginDetails = loginDetails;
            info = new UserInfo(loginDetails.userName, GenerateUniqueId());
        }

        public User(LoginDetails loginDetails, UserInfo info)
        {
            this.loginDetails = loginDetails;
            this.info = info;
        }

        public void SaveData()
        {
            File.WriteAllBytes(usersPath + this.loginDetails.userName + ".json", Encrypt(JsonConvert.SerializeObject(this)));

            //File.WriteAllText(usersPath + this.loginDetails.userName + ".json", JsonConvert.SerializeObject(this));
        }

        public static User TryLoadUserInfo(string userName)
        {
            if (DoesUserExist(userName))
                return JsonConvert.DeserializeObject<User>(Decrypt(File.ReadAllBytes(usersPath + userName + ".json")));

            return null;
        }

        private static byte[] Encrypt(string data) // Encryption src - https://www.c-sharpcorner.com/article/aes-encryption-in-c-sharp/
        {
            ICryptoTransform encryptor = new AesManaged().CreateEncryptor(key, iv);

            using (AesManaged aes = new AesManaged())
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter writer = new StreamWriter(cryptoStream))
                        {
                            writer.Write(data);
                        }
                    }

                    return stream.ToArray();
                }
            }

            //ICryptoTransform encryotor = new AesManaged().CreateEncryptor(key, iv);
            //
            //MemoryStream stream = new MemoryStream();
            //CryptoStream cryptoStream = new CryptoStream(stream, encryotor, CryptoStreamMode.Write);
            //new StreamWriter(cryptoStream).Write(data);
            //
            //return stream.ToArray();
        }

        private static string Decrypt(byte[] bytes)
        {
            using (AesManaged aes = new AesManaged())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cryptoStream))
                            return reader.ReadToEnd();
                    }
                }
            }

        }

        // TODO: Broadcast changes to other users. Maybe don't save data until the user disconnects or server shuts down.
        public void ChangeName(string newName) { this.info.name = newName; SaveData(); }
        public void ChangeImage(byte[] newImgData) { this.info.image = newImgData; SaveData(); }


        public static bool CreateAccount(LoginDetails loginDetails, UserInfo info, out User user)
        {
            info.uniqueId = GenerateUniqueId();
            return (user = !DoesUserExist(loginDetails.userName) ? new User(loginDetails, info) : null) != null;
        }

        public static bool DoesUserExist(string userName)
        {
            return File.Exists(usersPath + userName + ".json");
        }

        public static int GenerateUniqueId() // Not safe but it will do.
        {
            Random rand = new Random();
            return rand.Next();
        }
    }
}
