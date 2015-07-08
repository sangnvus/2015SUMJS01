using FlyAwayPlus.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FlyAwayPlus.Helpers
{
    public class GraphDatabaseHelpers
    {
        private static GraphClient client = new GraphClient(new Uri(ConfigurationManager.AppSettings["dbGraphUri"]));

        public static void insertUser(User user)
        {
            
            // auto incrementID
            user.userID = getGlobalIncrementID();

            client.Connect();
            var userRef = client.Cypher.
                        Create("(user:user {newUser})").
                        WithParam("newUser", user)
                        .Return<User> ("user")
                        .Results.Single();
        }

        public static User getUser(int typeID, string email)
        {
            client.Connect();
            List<User> listUser = new List<User>();
            listUser = client.Cypher.Match("(u:user {typeID:" + typeID + ", email: '" + email + "'})")
                        .Return<User>("u")
                        .Results.ToList();
            if (listUser.Count == 0)
            {
                return null;
            }

            return listUser.First();
        }

        public static int getGlobalIncrementID()
        {
            client.Connect();
            var uniqueID = client.Cypher.Merge("(id:GlobalUniqueId)")
                            .OnCreate().Set("id.count = 1")
                            .OnMatch().Set("id.count = id.count + 1")
                            .Return<int>("id.count AS uniqueID")
                            .Results.Single();

            return uniqueID;
        }
    }
}