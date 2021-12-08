using Newtonsoft.Json;
using System;
using System.IO;
using User;

namespace Server
{
    [Serializable]
    public class User
    {
        private static readonly string usersPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName + "\\Data\\Users\\";

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

        public static bool CreateAccount(LoginDetails loginDetails, UserInfo info, out User user)
        {
            return (user = !DoesUserExist(loginDetails.userName) && loginDetails != null && info != null ? new User(loginDetails, info) : null) != null;
        }

        public void SaveData()
        {
            File.WriteAllText(usersPath + this.loginDetails.userName + ".json", JsonConvert.SerializeObject(this));
        }

        public static User TryLoadUserInfo(string userName)
        {
            if (DoesUserExist(userName))
                return JsonConvert.DeserializeObject<User>(File.ReadAllText(usersPath + userName + ".json"));

            return null;
        }

        private static bool DoesUserExist(string userName)
        {
            return File.Exists(usersPath + userName + ".json");
        }

        private static int GenerateUniqueId()
        {
            Random rand = new Random();
            return rand.Next();
        }
    }
}
