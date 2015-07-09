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

        public static Post findPost(int id, User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * match(u1:user {userID:@userID})-[:friend]->(u2:user)
                   with u1, u2
                   match(p:post {postID:@postID}) 
                   where (u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')
                   return p
                 */
            Post p = null;
            client.Connect();
            p = client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:friend]->(u2:user)")
                            .With("u1, u2")
                            .Match("(p:post {postID:" + id + "})")
                            .Where("(u1-[:has]->p) or (p.privacy='friend' and u2-[:has]->p) or (p.privacy = 'public')")
                            .ReturnDistinct<Post>("p")
                            .Results.Single();

            return p;
        }

        public static User searchUser(Post post) {
            User user = null;
            client.Connect();
            user = client.Cypher.Match("(u:user)-[:has]->(p:post)")
                            .Where("p.postID=" + post.postID)
                            .ReturnDistinct<User>("u")
                            .Results.Single();
            return user;
        }

        public static List<Comment> findComment(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - comment of post
                 * 
                 * MATCH(p:post {postID:@postID})-[:has]->(c:comment)
                    return c
                 */
            List<Comment> list = null;
            client.Connect();
            list = client.Cypher.Match("(p:post {postID:" + post.postID + "})-[:has]->(c:comment)")
                            .Return<Comment>("c")
                            .OrderBy("c.dateCreated")
                            .Results.ToList();
            return list;
        }

        public static User findUser(Comment comment)
        {
            /*
                 * Query:
                 * Find:
                 *     - comment of post
                 * 
                 * MATCH(u:user)-[:has]->(c:comment{commentID:@commentID})
                    return u
                 */
            User user = null;
            client.Connect();
            user = client.Cypher.Match("(u:user)-[:has]->(c:comment{commentID:" + comment.commentID + "})")
                            .Return<User>("u")
                            .Results.Single();
            return user;
        }
    }
}