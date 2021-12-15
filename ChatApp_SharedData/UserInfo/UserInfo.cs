using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace User
{
    [Serializable]
    public class LoginDetails
    {
        public string userName { get; set; }
        public string password { get; set; }

        public LoginDetails() { }

        public LoginDetails(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }
    }

    [Serializable]
    public class UserInfo // maybe add some more info like description, img, etc;
    {
        public string name;
        public int uniqueId;
        public byte[] image;

        public UserInfo() {}

        public UserInfo(string name)
        {
            this.name = name;
        }

        public UserInfo(string name, byte[] imgData)
        {
            this.name = name;
            this.image = imgData;
        }

        public UserInfo(string name, int uniqueId, byte[] imgData = null)
        {
            this.name = name;
            this.uniqueId = uniqueId;
            this.image = imgData;
        }
    }
}
