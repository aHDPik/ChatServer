using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ChatContext:DbContext
    {

        public DbSet<UserData> UserData { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=ChatDb;Trusted_Connection=True;");
        }

        public async Task<bool> SendMessage(string senderName, string receiverName, string message)
        {
            UserData sender = await UserData.SingleOrDefaultAsync(usr => usr.Name == senderName);
            if(sender == null)
                return false;
            UserData receiver = await UserData.SingleOrDefaultAsync(usr => usr.Name == receiverName);
            if (receiver == null)
                return false;
            Message msg = new Message()
            {
                Receiver = receiver,
                Sender = sender,
                Text = message,
                Timestamp = DateTime.Now
            };
            await Messages.AddAsync(msg);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<Message>> GetAllMyMessages(string username, DateTime startTime)
        {
            IQueryable<Message> messages = (from msg in Messages
                                           where msg.Timestamp > startTime && 
                                           msg.Receiver.Name == username
                                           select msg).Include(m=>m.Sender);
            return await messages.ToListAsync();
        }

        public async Task<bool> RegisterUser(string username)
        {
            if (UserData.Any(usr => usr.Name == username))
                return false;
            UserData user = new UserData()
            {
                Name = username
            };
            await UserData.AddAsync(user);
            await SaveChangesAsync();
            return true;
        }
    }
}
