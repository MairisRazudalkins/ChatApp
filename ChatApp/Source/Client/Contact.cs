using System;
using System.Collections.Generic;

using User;
using Packets;

namespace ChatApp
{
    public class Contact
    {
        private static Action<bool, Contact> updateContactCallback;
        private static List<Contact> contacts = new List<Contact>();
        private List<Message> messages = new List<Message>();

        private UserInfo contactInfo;

        public Contact(UserInfo contactInfo)
        {
            this.contactInfo = contactInfo;
            contacts.Add(this);
        }

        public void SendMessage(Message msg)
        {
            Client.GetInst().SendPacket(new MsgPacket(0, contactInfo.uniqueId, msg));
            messages.Add(msg);
        }

        public void SendImgMessage(ImgMessage msg)
        {
            Client.GetInst().SendPacket(new ImgMsgPacket(0, contactInfo.uniqueId, msg.imgData, (Message)msg));
            messages.Add(msg);
        }

        public void AddMessage(Message msg)
        {
            messages.Add(msg);
            Messenger.GetInst().AddMessage(contactInfo.uniqueId, msg);
        }

        public static void OnReceiveMessage(int senderId, Message msg)
        {
            Contact contact = FindContact(senderId);

            if (contact != null)
            {
                contact.AddMessage(msg);
            }
        }

        //public void SendImageMessage(byte[] image, string msg) // send image as msg to target.
        //{
        //    Client.GetInst().SendPacket(new ImgMsgPacket(0, contactInfo.uniqueId, image, msg));
        //}


        public List<Message> GetMessages() { return messages; }

        public List<Message> GetLatestMessages()
        {
            if (this.messages.Count <= 10)
                return this.messages;

            List<Message> messages = new List<Message>();

            int index = 0;

            for (int i = this.messages.Count - 11; i < this.messages.Count; i++)
            {
                messages.Add(this.messages[i]);
                index++;
            }

            return messages;
        }

        public UserInfo GetInfo() { return contactInfo; }

        public static Contact FindContact(int id)
        {
            foreach (Contact contact in contacts)
                if (contact.contactInfo.uniqueId == id)
                    return contact;

            return null;
        }

        public static void RemoveContact(int id)
        {
            Contact contact = FindContact(id);

            if (contact != null)
            {
                contacts.Remove(contact);

                if (updateContactCallback != null)
                    updateContactCallback(false, contact);
            }
        }

        public static void RemoveContact(Contact contact)
        {
            contacts.Remove(contact); 

            if (updateContactCallback != null) 
                updateContactCallback(false, contact);
        }

        public static void AddContact(Contact contact)
        {
            contacts.Add(contact); 

            if (updateContactCallback != null) 
                updateContactCallback(true, contact);
        }
        public static void AssignCallback(Action<bool, Contact> callback) { updateContactCallback = callback; }

        public static List<Contact> GetContacts()  { return contacts; }

        public void UpdateImage(byte[] imgData) // THIS IS A BIG MESS ALL BECAUSE OF DISPACHER ISSUE!
        {
            Messenger.GetInst().UpdateContactImage(this, (this.contactInfo.image = imgData));
        }
    }
}
