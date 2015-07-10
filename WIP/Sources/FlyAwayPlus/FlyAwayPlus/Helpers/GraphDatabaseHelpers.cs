using FlyAwayPlus.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using FlyAwayPlus.Models.Relationships;

namespace FlyAwayPlus.Helpers
{
    public class GraphDatabaseHelpers
    {
        private static readonly GraphClient Client = new GraphClient(new Uri(ConfigurationManager.AppSettings["dbGraphUri"]));

        public static void InsertUser(User user)
        {
            // Auto increment Id.
            user.userID = GetGlobalIncrementId();

            Client.Connect();
            var userRef = Client.Cypher.
                        Create("(user:user {newUser})").
                        WithParam("newUser", user)
                        .Return<User> ("user")
                        .Results.Single();
        }

        public static void InsertPost(Post post, Photo photo, Place place)
        {
            // Auto increment Id.
            post.postID = GetGlobalIncrementId();
            photo.photoID = GetGlobalIncrementId();
            place.placeID = GetGlobalIncrementId();

            Client.Connect();

            NodeReference<Post> postRef = Client.Create(post);

            NodeReference<Photo> photoRef = Client.Create(photo);

            NodeReference<Place> placeRef = Client.Create(place);

            Client.CreateRelationship(postRef, new HasRelationship(photoRef));
            Client.CreateRelationship(postRef, new AtRelationship(placeRef));
        }

        public static User GetUser(int typeId, string email)
        {
            Client.Connect();
            var listUser = Client.Cypher.Match("(u:user {typeID:" + typeId + ", email: '" + email + "'})")
                .Return<User>("u")
                .Results.ToList();
            return listUser.Count == 0 ? null : listUser.First();
        }

        public static int GetGlobalIncrementId()
        {
            Client.Connect();
            var uniqueId = Client.Cypher.Merge("(id:GlobalUniqueId)")
                            .OnCreate().Set("id.count = 1")
                            .OnMatch().Set("id.count = id.count + 1")
                            .Return<int>("id.count AS uniqueID")
                            .Results.Single();

            return uniqueId;
        }
    }
}