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
            client.Connect();
            var userRef = client.Cypher.
                        Create("(user:user {userDetail})").
                        WithParam("userDetail", user)
                        .Return<User> ("user")
                        .Results.Single();
        }
    }
}