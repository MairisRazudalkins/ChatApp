using System;
using System.Collections.Generic;
using User;
using Packets;

namespace ChatApp
{
    class Contact
    {
        private static List<Contact> contacts = new List<Contact>();

        private UserInfo contactInfo;

        public Contact(UserInfo contactInfo) 
        {
            this.contactInfo = contactInfo;
            contacts.Add(this);
        }

        public void SendMessage(string msg)
        {
            Client.GetInst().SendPacket(new MsgPacket(0, contactInfo.uniqueId, msg));
        }

        public void SendImageMessage(byte[] image, string msg) // send image as msg to target.
        {
            Client.GetInst().SendPacket(new ImgMsgPacket(0, contactInfo.uniqueId, image, msg));
        }

        public UserInfo GetInfo() { return contactInfo; }

        public static void RemoveContact(Contact contact)  { contacts.Remove(contact); }
        public static List<Contact> GetContacts()  { return contacts; }
    }
}
