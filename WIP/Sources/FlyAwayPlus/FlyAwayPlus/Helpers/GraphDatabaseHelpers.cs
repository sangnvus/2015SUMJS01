using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using FlyAwayPlus.Models;
using FlyAwayPlus.Models.Relationships;
using Neo4jClient;

namespace FlyAwayPlus.Helpers
{
    public class GraphDatabaseHelpers : SingletonBase<GraphDatabaseHelpers>
    {
        private readonly GraphClient _client;

        private GraphDatabaseHelpers()
        {
            _client = new GraphClient(new Uri(ConfigurationManager.AppSettings["dbGraphUri"]));
        }

        public string GetEmailByUserId(int userId)
        {
            _client.Connect();
            string email = _client.Cypher.Match("(u:user {userID:" + userId + "})")
                                    .Return<string>("u.email")
                                    .Results.Single();
            return email;
        }

        public void LockUser(int userId)
        {
            /*
             * MATCH (n:user { userID: userId })
                SET n.status = 'lock'
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:user { userID: " + userId + " })")
                           .Set("n.status = 'lock' RETURN n")
                           .ExecuteWithoutResults();
        }

        public void LockPost(int postId)
        {
            /*
             * MATCH (n:post { postID: postId })
                SET n.status = 'lock'
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:post { postID: " + postId + " })")
                           .Set("n.privacy = 'lock' RETURN n")
                           .ExecuteWithoutResults();
        }

        public void UnlockUser(int userId)
        {
            /*
             * MATCH (n:user { userID: userId })
                SET n.status = 'lock'
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:user { userID: " + userId + " })")
                           .Set("n.status = 'active' RETURN n")
                           .ExecuteWithoutResults();
        }

        public void InsertUser(ref User user)
        {
            // Auto increment Id.
            user.userID = GetGlobalIncrementId();
            _client.Connect();

            _client.Cypher
                   .Create("(user:user {newUser})")
                   .WithParam("newUser", user)
                   .ExecuteWithoutResults();
        }

        public void InsertReportPost(ReportPost reportPost)
        {
            // Auto increment Id.
            reportPost.reportID = GetGlobalIncrementId();

            _client.Connect();
            _client.Cypher.Create("(p:reportPost {newReportPost})")
                .WithParam("newReportPost", reportPost)
                .ExecuteWithoutResults();

            _client.Cypher.Match("(p:reportPost {reportID:" + reportPost.reportID + "}), (u:user {userID: " + reportPost.userReportID + "})")
                                 .Create("(p)-[r:REPORT_BY]->(u)")
                                 .ExecuteWithoutResults();

            _client.Cypher.Match("(p:reportPost {reportID:" + reportPost.reportID + "}), (u:user {userID: " + reportPost.userReportedID + "})")
                                 .Create("(p)-[r:REPORT_TO]->(u)")
                                 .ExecuteWithoutResults();
        }

        public void InsertReportUser(ReportUser reportUser)
        {
            // Auto increment Id.
            reportUser.reportID = GetGlobalIncrementId();

            _client.Connect();
            _client.Cypher.Create("(p:reportUser {newReportUser})")
                .WithParam("newReportUser", reportUser)
                                 .ExecuteWithoutResults();

            _client.Cypher.Match("(p:reportUser {reportID:" + reportUser.reportID + "}), (u:user {userID: " + reportUser.userReportID + "})")
                                 .Create("(p)-[r:REPORT_BY]->(u)")
                                 .ExecuteWithoutResults();

            _client.Cypher.Match("(p:reportPost {reportID:" + reportUser.reportID + "}), (u:user {userID: " + reportUser.userReportedID + "})")
                                 .Create("(p)-[r:REPORT_TO]->(u)")
                                 .ExecuteWithoutResults();
        }

        public bool IsLike(int postId, int userId)
        {
            _client.Connect();
            int like = _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:LIKE]->(p:post {postID:" + postId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return like != 0;
        }

        public bool IsFriend(int userId, int otherUserId)
        {
            _client.Connect();
            int friend = _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:FRIEND]->(u1: user{userID: " + otherUserId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return friend != 0;
        }

        public string GetFriendType(int userId, int otherUserId)
        {
            // Auto increment Id
            if (IsFriend(userId, otherUserId))
            {
                return "friend";
            }

            _client.Connect();
            int friend = _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:FRIEND_REQUEST]->(u1: user{userID: " + otherUserId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return friend != 0 ? "request" : "none";
        }

        public bool IsDislike(int postId, int userId)
        {
            // Auto increment Id

            _client.Connect();
            int dislike = _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:DISLIKE]->(p:post {postID:" + postId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return dislike != 0;
        }

        public bool IsWish(int postId, int userId)
        {
            _client.Connect();
            int wish = _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:WISH]->(p:post {postID:" + postId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return wish != 0;
        }

        public bool IsInWishist(int placeId, int userId)
        {
            _client.Connect();
            int wish = _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "})-[r:WISH]->(p:place {placeID:" + placeId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.FirstOrDefault();

            return wish != 0;
        }

        public bool AddToWishlist(int placeId, int userId)
        {
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {userID:" + userId + "}), (p:place {placeID:" + placeId + "})")
                                    .Create("(u)-[:WISH]->(p)")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool RemoveFromWishlist(int placeId, int userId)
        {
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:WISH]->(p:place {placeID:" + placeId + "})")
                                    .Delete("r")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool AddToWishList(int postId, int userId)
        {
            _client.Connect();

            NodeReference<Post> postRef = GetNodePost(postId).Reference;
            Node<User> userNode = GetNodeUser(userId);
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                _client.CreateRelationship(userRef, new UserWishPostRelationship(postRef));
                return true;
            }
            return false;
        }

        public bool RemoveFromWishList(int postId, int userId)
        {
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:WISH]->(p:post {postID:" + postId + "})")
                                    .Delete("r")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public int CountUserComment(int postId)
        {
            /*
             * Match (p:post {postID:@postID})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)
                return COUNT (DISTINCT u)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {postID:" + postId + "})<-[r:COMMENTED]-(u:user)")
                                .Return<int>("COUNT(distinct u)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int CountComment(int postId)
        {
            /*
             * match(p:post {postID:@postID})-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)
                    return Length(collect(c1)) as CommentNumber
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {postID:" + postId + "})-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)")
                                .Return<int>("Length(collect(c1)) as CommentNumber")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int CountDislike(int postId)
        {
            /*
             * Match (p:post {postID:@postID})<-[r:DISLIKE]-(u:user)
                return COUNT(DISTINCT c)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {postID:" + postId + "})<-[r:DISLIKE]-(u:user)")
                                .Return<int>("COUNT(distinct r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int CountLike(int postId)
        {
            /*
             * Match (p:post {postID:@postID})<-[r:LIKE]-(u:user)
                return COUNT(DISTINCT r)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {postID:" + postId + "})<-[r:LIKE]-(u:user)")
                                .Return<int>("COUNT(DISTINCT r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int CountMutualFriend(int userId, int otherUserId)
        {
            /*
             * optional match (u:user {userID:@userID})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user{userID:@otherUserID})
                    With count(DISTINCT mf) AS mutualFriends
                    RETURN mutualFriends
             */
            try
            {
                _client.Connect();
                return _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user{userID:" + otherUserId + "})")
                                .With("count(DISTINCT mf) AS mutualFriends")
                                .Return<int>("mutualFriends")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int CountUser()
        {
            /*
             * Match (p:post {postID:@postID})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)
                return COUNT (DISTINCT u)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(u:user")
                                .Return<int>("COUNT(distinct u)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int FindLike(int userId, int postId)
        {
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:LIKE]->(p:post {postID:" + postId + "})")
                                .Return<int>("COUNT(r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public int FindDislike(int userId, int postId)
        {
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:DISLIKE]->(p:post {postID:" + postId + "})")
                                .Return<int>("COUNT(r)")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }
        }

        public bool DeleteLike(int userId, int postId)
        {
            /**
             * Match(u:user {userID:@userID})-[r:like]->(p:post {postID:@postID})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:LIKE]->(p:post {postID:" + postId + "})")
                                .Delete("r")
                                .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool DeleteDislike(int userId, int postId)
        {
            /**
             * Match(u:user {userID:@userID})-[r:dislike]->(p:post {postID:@postID})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:DISLIKE]->(p:post {postID:" + postId + "})")
                                .Delete("r")
                                .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool DeleteReportPost(int reportID)
        {
            /**
             * Match(u:user {userID:@userID})-[r:dislike]->(p:post {postID:@postID})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:reportPost {reportID:" + reportID + "})-[r]-()")
                                .Delete("n,r")
                                .ExecuteWithoutResults();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool DeleteReportUser(int reportID)
        {
            /**
             * Match(u:user {userID:@userID})-[r:dislike]->(p:post {postID:@postID})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:reportUser {reportID:" + reportID + "})-[r]-()")
                                .Delete("n,r")
                                .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool InsertDislike(int userId, int postId)
        {
            Node<User> userNode = GetNodeUser(userId);

            NodeReference<Post> postRef = _client.Cypher.Match("(p:post {postID:" + postId + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                _client.CreateRelationship(userRef, new UserDislikePostRelationship(postRef, new { dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat), activtyID = GetActivityIncrementId() }));
                return true;
            }
            return false;
        }

        public bool InsertLike(int userId, int postId)
        {
            Node<User> userNode = GetNodeUser(userId);

            NodeReference<Post> postRef = _client.Cypher.Match("(p:post {postID:" + postId + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                _client.CreateRelationship(userRef, new UserLikePostRelationship(postRef, new { dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat), activtyID = GetActivityIncrementId() }));
                return true;
            }
            return false;
        }

        public Node<User> GetNodeUser(int id)
        {
            _client.Connect();
            return _client.Cypher.Match("(u:user {userID: " + id + "})").Return<Node<User>>("u").Results.FirstOrDefault();
        }

        public Node<Post> GetNodePost(int id)
        {
            _client.Connect();
            return _client.Cypher.Match("(p:post {postID: " + id + "})").Return<Node<Post>>("p").Results.FirstOrDefault();
        }

        public User GetUser(int typeId, string email)
        {
            _client.Connect();
            var user = _client.Cypher.Match("(u:user {typeID:" + typeId + ", email: '" + email + "'})")
                .Return<User>("u")
                .Results.FirstOrDefault();
            return user;
        }

        public int GetGlobalIncrementId()
        {
            _client.Connect();
            var uniqueId = _client.Cypher.Merge("(id:GlobalUniqueId)")
                            .OnCreate().Set("id.count = 1")
                            .OnMatch().Set("id.count = id.count + 1")
                            .Return<int>("id.count AS uniqueID")
                            .Results.Single();

            return uniqueId;
        }

        public int GetActivityIncrementId()
        {
            _client.Connect();
            var uniqueId = _client.Cypher.Merge("(id:ActivityUniqueId)")
                            .OnCreate().Set("id.count = 1")
                            .OnMatch().Set("id.count = id.count + 1")
                            .Return<int>("id.count AS uniqueID")
                            .Results.Single();

            return uniqueId;
        }

        public User FindUser(Comment comment)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has comment
                 * 
                 * MATCH(u:user)-[:has]->(c:comment{commentID:@commentID})
                    return u
                 */
            _client.Connect();
            var user = _client.Cypher.Match("(u:user)-[:CREATE]->(c:comment{commentID:" + comment.commentID + "})")
                .Return<User>("u")
                .Results.Single();
            return user;
        }

        public User FindUser(int id)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has id
                 * 
                 * MATCH(u:user {userID:@userID})
                    return u
                 */
            User user;
            try
            {
                _client.Connect();
                user = _client.Cypher.Match("(u:user {userID:" + id + "})")
                                .Return<User>("u")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                user = null;
            }
            return user;
        }

        public User FindUser(string email)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has email
                 * 
                 * MATCH(u:user {userID:@userEmail})
                    return u
                 */
            User user;
            try
            {
                _client.Connect();
                user = _client.Cypher.Match("(u:user {email:'" + email + "'})")
                                .Return<User>("u")
                                .Results.Single();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                user = null;
            }
            return user;
        }

        public List<User> ListAllUsers()
        {

            /*
                 * Query:
                 * Find:
                 *     - all current user
                 * 
                 * match(u1:user)
                    return u1;
                 */
            _client.Connect();
            var listAllUsers = _client.Cypher.OptionalMatch("(u1:user)")
                .ReturnDistinct<User>("u1")
                .Results.ToList();
            return listAllUsers;
        }

        public List<ReportPost> ListAllReportPosts()
        {

            /*
                 * Query:
                 * Find:
                 *     - all current report post
                 * 
                 * match(u1:repost)
                    return u1;
                 */
            _client.Connect();
            var listAllReportPosts = _client.Cypher.OptionalMatch("(u1:reportPost)")
                .ReturnDistinct<ReportPost>("u1")
                .Results.ToList();
            return listAllReportPosts;
        }

        public List<ReportUser> ListAllReportUsers()
        {

            /*
                 * Query:
                 * Find:
                 *     - all current report user
                 * 
                 * match(u1:reportUser)
                    return u1;
                 */
            _client.Connect();
            var listAllReportPosts = _client.Cypher.OptionalMatch("(u1:reportUser)")
                .ReturnDistinct<ReportUser>("u1")
                .Results.ToList();
            return listAllReportPosts;
        }

        public List<User> FindFriend(User user)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * match(u1:user{userID:@userID})-[m:friend]->(u2:user)
                    return u2;
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u1:user {userID:" + user.userID + "})-[:FRIEND]->(u2:user)")
                .ReturnDistinct<User>("u2")
                .Results.ToList();
            listUser.RemoveAll(item => item == null);
            return listUser;
        }

        public List<User> SuggestFriend(int userId, int limit = 5)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * optional match (u:user {userID:@userID})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user)
                    WHERE NOT(u-[:FRIEND]->other)
                    WITH other,count(DISTINCT mf) AS mutualFriends
                    ORDER BY mutualFriends DESC
                    LIMIT @limit
                    RETURN other
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user)")
                .Where("NOT(u-[:FRIEND]->other)")
                .With("other,count(DISTINCT mf) AS mutualFriends")
                .OrderByDescending("mutualFriends")
                .ReturnDistinct<User>("other")
                .Limit(limit)
                .Results.ToList();
            listUser.RemoveAll(item => item == null);

            if (listUser.Count < limit)
            {
                listUser.AddRange(SuggestNonRelationshipUser(userId, limit - listUser.Count));

            }
            return listUser;
        }

        public bool IsVisitedPlace(int userId, int placeId)
        {
            // Auto increment Id

            _client.Connect();
            int relation = _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:VISITED]->(p:place {placeID:" + placeId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return relation != 0;
        }

        public int NumberOfPost(int placeId)
        {
            // Auto increment Id

            _client.Connect();
            int count = _client.Cypher.Match("(po:post)-[r:AT]->(pl:place {placeID:" + placeId + "})")
                                   .Return<int>("COUNT(Distinct po)")
                                   .Results.Single();

            return count;
        }

        public List<Place> SuggestPlace(int limit = 5)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * optional match (pl:place)<-[:AT]-(po:post)
                    With count(DISTINCT po) as number, pl, po
                    return pl
                 */
            _client.Connect();
            var listPlace = _client.Cypher.OptionalMatch("(pl:place)<-[:AT]-(po:post)")
                .With("count(DISTINCT po) as number, pl, po")
                .OrderByDescending("number")
                .ReturnDistinct<Place>("pl")
                .Limit(limit)
                .Results.ToList();
            listPlace.RemoveAll(item => item == null);

            return listPlace;
        }

        public List<User> SuggestNonRelationshipUser(int userId, int limit = 5)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * optional match (u:user {userID:@userID})-[:FRIEND]->(u1:user), (other:user)
                    WHERE NOT(u-[:FRIEND]->other) AND NOT(u1-[:FRIEND]->other)
                    RETURN other
                    limit @limit
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "})-[:FRIEND]->(u1:user), (other:user)")
                .Where("NOT(u-[:FRIEND]->other) AND NOT(u1-[:FRIEND]->other)")
                .ReturnDistinct<User>("other")
                .Limit(limit)
                .Results.ToList();
            listUser.RemoveAll(item => item == null);

            if (listUser.Count == 0)
            {
                listUser = _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "}), (other:user)")
                            .Where("u.userID <> other.userID")
                            .ReturnDistinct<User>("other")
                            .Limit(limit)
                            .Results.ToList();
            }

            listUser.RemoveAll(item => item == null);
            return listUser;
        }

        public List<User> FindFriend(int userId)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * match(u1:user{userID:@userID})-[m:friend]->(u2:user)
                    return u2;
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u1:user {userID:" + userId + "})-[:FRIEND]-(u2:user)")
                .ReturnDistinct<User>("u2")
                .Results.ToList();
            listUser.RemoveAll(item => item == null);
            return listUser;
        }

        public List<Post> FindLimitWishlist(User user, int skip, int limit)
        {
            /*
                 * Query:
                 * Find:
                 *     - wishlist
                 * 
                 * match(u1:user {userID:@userID})-[:wish]->(p:post)
                   return p
                   orderby p.dateCreated
                 */
            _client.Connect();
            var listPost = _client.Cypher.Match("(u1:user {userID:" + user.userID + "})-[:WISH]->(p:post)")
                .ReturnDistinct<Post>("p")
                .OrderByDescending("p.dateCreated")
                .Skip(skip)
                .Limit(limit)
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public void InsertPost(User user, Post post, List<Photo> photos, Place place, Video video)
        {
            // Auto increment Id.
            SpecifyIds(ref post, ref photos, ref place, ref video);

            _client.Connect();

            _client.Cypher.Create("(p:post {newPost})")
                         .WithParam("newPost", post)
                         .ExecuteWithoutResults();

            foreach (var photo in photos)
            {
                _client.Cypher.Create("(p:photo {newPhoto})")
                             .WithParam("newPhoto", photo)
                             .ExecuteWithoutResults();
            }

            _client.Cypher.Create("(p:place {newPlace})")
                         .WithParam("newPlace", place)
                         .ExecuteWithoutResults();

            if (video != null)
            {
                _client.Cypher.Create("(v:video {newVideo})")
                         .WithParam("newVideo", video)
                         .ExecuteWithoutResults();
            }

            Node<User> userNode = GetNodeUser(user.userID);
            if (userNode != null)
            {
                var existingPlace = FindExistingPlace(place);

                if (existingPlace == null)
                {
                    _client.Cypher.Match("(po:post {postID:" + post.postID + "}), (pl:place {placeID: " + place.placeID + "})")
                             .Create("(po)-[r:AT]->(pl)")
                             .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.Match("(po:post {postID:" + post.postID + "}), (pl:place {placeID: " + existingPlace.placeID + "})")
                                 .Create("(po)-[r:AT]->(pl)")
                                 .ExecuteWithoutResults();
                }

                if (video != null)
                {
                    _client.Cypher.Match("(po:post {postID:" + post.postID + "}), (vi:video {videoID: " + video.videoID + "})")
                             .Create("(po)-[r:HAS]->(vi)")
                             .ExecuteWithoutResults();
                }

                foreach (var photo in photos)
                {
                    _client.Cypher.Match("(po:post {postID:" + post.postID + "}), (pt:photo {photoID: " + photo.photoID + "})")
                                 .Create("(po)-[r:HAS]->(pt)")
                                 .ExecuteWithoutResults();

                    _client.Cypher.Match("(u:user {userID:" + user.userID + "}), (p:photo {photoID: " + photo.photoID + "})")
                                 .Create("(u)-[r:HAS]->(p)")
                                 .ExecuteWithoutResults();
                }

                _client.Cypher.Match("(u:user {userID:" + user.userID + "}), (p:place {placeID: " + place.placeID + "})")
                             .Create("(u)-[r:VISITED]->(p)")
                             .ExecuteWithoutResults();

                //Client.CreateRelationship(userRef, new UserHasPostRelationship(postRef));
                /*
                 * Check User has post:
                 *  if yes do the following step:
                 *      1. DELETE the LATEST_POST relationship from user to oldPost
                 *      2. CREATE LATEST_POST relationship from user to newPost
                 *      3. CREATE PREV_POST relationship from newPost to oldPost
                 *      
                 *  else do the following step:
                 *      1. CREATE LATEST_POST relationship from user to newPost
                 */
                int oldPost = _client.Cypher.Match("(u:user{userID:" + user.userID + "})-[:LATEST_POST]->(p:post)")
                                    .Return<int>("COUNT (p)")
                                    .Results.Single();

                if (oldPost == 0)
                {
                    // CREATE New LATEST_POST
                    _client.Cypher.Match("(u:user{userID:" + user.userID + "}), (p1:post{postID:" + post.postID + "})")
                                    .Create("(u)-[:LATEST_POST]->(p1)")
                                    .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.Match("(u:user{userID:" + user.userID + "})-[r:LATEST_POST]->(p:post), (p1:post{postID:" + post.postID + "})")
                                    .Delete("r")
                                    .Create("(u)-[:LATEST_POST]->(p1)")
                                    .Create("(p1)-[:PREV_POST]->(p)")
                                    .ExecuteWithoutResults();
                }
            }
        }

        private void SpecifyIds(ref Post post, ref List<Photo> photos, ref Place place, ref Video video)
        {
            post.postID = GetGlobalIncrementId();
            foreach (var photo in photos)
            {
                photo.photoID = GetGlobalIncrementId();
            }

            place.placeID = GetGlobalIncrementId();
            if (video != null)
            {
                video.videoID = GetGlobalIncrementId();
            }
        }

        public User SearchUser(int postId)
        {
            /**
             * match (p:post{postID:1003})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                return u
             */
            _client.Connect();
            var user = _client.Cypher.Match("(p:post{postID:" + postId + "})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)")
                .ReturnDistinct<User>("u")
                .Results.Single();
            return user;
        }

        public List<Comment> FindComment(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - find comment of post
                 * 
                 * match (p:post{postID:@postID})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)
                    return c1
                 */
            _client.Connect();
            var list = _client.Cypher.Match("(p:post{postID:" + post.postID + "})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)")
                .Return<Comment>("c1")
                .OrderBy("c1.dateCreated")
                .Results.ToList();
            list.RemoveAll(item => item == null);
            return list;
        }

        public List<Post> FindPostOfUser(User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - all post of current user
                 * 
                 * match (u:user{userID:@userID})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)
                    return p1
                 */
            _client.Connect();
            var listPost = _client.Cypher.Match("(u:user {userID:" + user.userID + "})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)")
                .ReturnDistinct<Post>("p1")
                //.OrderByDescending("p.dateCreated")
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public Post FindPostByID(int postID)
        {
            /*
                 * Query:
                 * Find:
                 */
            _client.Connect();
            var post = _client.Cypher.Match("(p:post {postID:" + postID + "})")
                .ReturnDistinct<Post>("p")
                .Results.SingleOrDefault();
            return post;
        }

        public List<Photo> FindPhoto(int postId)
        {

            /*
                 * Query:
                 * Find:
                 *     - Find photo of post
                 * 
                 * match (po:post {postID:@postID})-[:has]->(ph:photo)
                    return ph
                 */

            _client.Connect();
            return _client.Cypher.Match("(po:post {postID:" + postId + "})-[:HAS]->(ph:photo)")
                            .Return<Photo>("ph")
                            .OrderBy("ph.photoID")
                            .Results
                            .ToList();
        }

        public Video FindVideo(int postId)
        {
            _client.Connect();

            return _client.Cypher.Match("(po:post {postID:" + postId + "})-[:HAS]->(v:video)")
                            .Return<Video>("v")
                            .OrderBy("v.videoID")
                            .Results
                            .ToList()
                            .FirstOrDefault();
        }

        public User FindUser(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has post
                 * 
                 * match (p:post{postID:@postID})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                    return u
                 */
            _client.Connect();
            var user = _client.Cypher.Match("(p:post{postID:" + post.postID + "})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)")
                .Return<User>("u")
                .Results.FirstOrDefault();
            return user;
        }

        public User FindUserByPostInRoom(Post post)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has post
                 * 
                 * match (p:post{postID:@postID})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                    return u
                 */
            _client.Connect();
            var user = _client.Cypher.Match("(p:post{postID:" + post.postID + "})<-[:CREATE]-(u:user)")
                .Return<User>("u")
                .Results.Single();
            return user;
        }

        public List<Post> SearchAllPost()
        {

            /*
                 * Query:
                 * Find:
                 *     - Search all post with privacy public
                 * 
                 * match (p:post)
                    where p.privacy='public'
                    return p
                 */

            _client.Connect();
            var listPost = _client.Cypher.Match("(p:post)")
                .Where("p.privacy='public'")
                .Return<Post>("p")
                .OrderByDescending("p.dateCreated")
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public Place FindPlace(Post po)
        {

            /*
                 * Query:
                 * Find:
                 *     - Find Place of post
                 * 
                 * match (po:post {postID:@postID})-[:at]->(pl:place)
                    return pl
                 */

            _client.Connect();
            Place place = null;
            try
            {
                place = _client.Cypher.Match("(po:post {postID:" + po.postID + "})-[:AT]->(pl:place)")
                    .Return<Place>("pl")
                    .OrderBy("pl.placeID")
                    .Results
                    .ToList()
                    .First();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                place = null;
            }
            return place;
        }

        public Place FindExistingPlace(Place place)
        {
            _client.Connect();
            return _client.Cypher.Match("(pl:place {longitude: '" + place.longitude + "', latitude: '" + place.latitude +
                                       "', name: '" + place.name + "'})")
                                .Return<Place>("pl")
                                .Results.FirstOrDefault();
        }

        public List<Post> FindPostOfOtherUser(User currentUser, User otherUser)
        {
            /*
                 * Query:
                 * Find:
                 *     - all post of otherUser that currentUser can view
                 * 
                 * match (u1:user{userID:@otherUserID})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post), (u2:user{userID:@userID})
                    where p1.privacy = 'public' or (p1.privacy = 'friend' and u1-[:FRIEND]-u2)
                    return p1
                 */
            _client.Connect();
            var listPost = _client.Cypher.Match("(u1:user {userID:" + otherUser.userID + "})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post), (u2:user {userID:" + currentUser.userID + "})")
                .Where("p1.privacy = 'public' or (p1.privacy = 'friend' and u1-[:FRIEND]-u2)")
                .ReturnDistinct<Post>("p1")
                //.OrderByDescending("p.dateCreated")
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public List<Post> FindLimitPostFollowing(User user, int skip, int limit)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * optional match (u1:user{userID:@userID})-[:LATEST_POST]-(a:post)-[:PREV_POST*0..]-(p1:post)
                    optional match (u1)-[:FRIEND]-(u2:user)-[:LATEST_POST]-(b:post)-[:PREV_POST*0..]-(p2:post)
                    with collect(distinct p1) as list1, collect(distinct p2) as list2
                    match (p3:post)
                    where p3.privacy = 'public' or (p3 in list1) or (p3 in list2 and p3.privacy='friend')
                    return distinct p3
                 */
            _client.Connect();
            var listPost = _client.Cypher.OptionalMatch("(u1:user{userID:" + user.userID + "})-[:LATEST_POST]-(a:post)-[:PREV_POST*0..]-(p1:post)")
                .OptionalMatch("(u1)-[:FRIEND]-(u2:user)-[:LATEST_POST]-(b:post)-[:PREV_POST*0..]-(p2:post)")
                .With("collect(distinct p1) as list1, collect(distinct p2) as list2")
                .Match("(p3:post)")
                .Where("p3.privacy = 'public' or (p3 in list1) or (p3 in list2 and p3.privacy='friend')")
                .ReturnDistinct<Post>("p3")
                .OrderByDescending("p3.dateCreated")
                .Skip(skip)
                .Limit(limit)
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public Post FindPost(int id, User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * MATCH (u:user { userID:@userID }),(p:post { postID: @postID}), c = shortestPath((u)-[:LATEST_POST|:PREV_POST|:FRIEND*..]-(p))
                    RETURN COUNT(c)
                 */
            _client.Connect();
            var p = _client.Cypher.Match("(p:post {postID:" + id + "})")
                .ReturnDistinct<Post>("p")
                .Results.SingleOrDefault();
            if (p == null || !p.privacy.Equals("public"))
            {
                int path = _client.Cypher.Match("(u:user {userID:" + user.userID + "}),(p:post { postID: " + id + "}), c = shortestPath((u)-[:LATEST_POST|:PREV_POST|:FRIEND*..]-(p))")
                            .ReturnDistinct<int>("COUNT(c)")
                            .Results.Single();
                if (path != 0)
                {
                    return p;
                }
                return null;
            }
            else
            {
                return p;
            }
        }

        public List<Post> SearchLimitPost(int skip, int limit)
        {
            /*
                 * Query:
                 * Find:
                 *     - Search limit post with privacy public
                 * 
                 * match (p:post)
                    where p.privacy='public'
                    return p
                 */

            _client.Connect();
            var listPost = _client.Cypher.Match("(p:post)")
                .Where("p.privacy='public'")
                .Return<Post>("p")
                .OrderByDescending("p.dateCreated")
                .Skip(skip)
                .Limit(limit)
                .Results.ToList();

            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public bool InsertComment(int postId, Comment comment, int userId)
        {
            // Auto increment Id
            comment.commentID = GetGlobalIncrementId();

            _client.Connect();
            NodeReference<Comment> comRef = _client.Cypher.Create("(c:comment {newComment})").
                        WithParam("newComment", comment)
                        .Return<Node<Comment>>("c")
                        .Results.Single().Reference;

            Node<User> userNode = GetNodeUser(userId);
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                _client.CreateRelationship(userRef, new UserCreateCommentRelationship(comRef));
                /*
                 * Create Commented relationship
                 * 
                 * match (u:user{userID:@otherUserID})-[r:COMMENTED]->(p:post{postID:@postID})
                    delete r
                    create (u)-[r1:COMMENTED {dateCreated: @now, activityID : @activityID}]->(p)
                 */
                User otherUser = FindUser(comment);
                _client.Cypher.Match("(u:user{userID:" + otherUser.userID + "})-[r:COMMENTED]->(p:post{postID:" + postId + "})")
                            .Delete("r").
                            ExecuteWithoutResults();

                _client.Cypher.Match("(u:user{userID:" + otherUser.userID + "}), (p:post{postID:" + postId + "})")
                            .Create("(u)-[r1:COMMENTED {dateCreated: '" + DateTime.Now.ToString(FapConstants.DatetimeFormat) + "', activityID : " + GetActivityIncrementId() + "}]->(p)")
                            .ExecuteWithoutResults();

                //Client.CreateRelationship(postRef, new PostHasCommentRelationship(comRef));
                /*
                 * Check post has comment:
                 *  if yes do the following step:
                 *      1. DELETE the LATEST_COMMENT relationship from user to oldPost
                 *      2. CREATE LATEST_COMMENT relationship from user to newPost
                 *      3. CREATE PREV_COMMENT relationship from newPost to oldPost
                 *      
                 *  else do the following step:
                 *      1. CREATE LATEST_COMMENT relationship from user to newPost
                 */
                int oldComment = _client.Cypher.Match("(p:post{postID:" + postId + "})-[:LATEST_COMMENT]->(c:comment)")
                                    .Return<int>("COUNT (c)")
                                    .Results.Single();

                if (oldComment == 0)
                {
                    // CREATE New LATEST_COMMENT
                    _client.Cypher.Match("(p:post{postID:" + postId + "}), (c:comment{commentID:" + comment.commentID + "})")
                                    .Create("(p)-[:LATEST_COMMENT]->(c)")
                                    .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.Match("(p:post{postID:" + postId + "})-[r:LATEST_COMMENT]->(c:comment), (c1:comment{commentID:" + comment.commentID + "})")
                                    .Delete("r")
                                    .Create("(p)-[:LATEST_COMMENT]->(c1)")
                                    .Create("(c1)-[:PREV_COMMENT]->(c)")
                                    .ExecuteWithoutResults();
                }
                return true;
            }
            return false;
        }

        public List<Notification> GetNotification(int userId, int activityId = 0, int limit = 5)
        {
            /*
                * Query:
                * Find:
                *     - Search limit post with privacy public
                * 
                * optional match (u:user {userID:10000})-[:LATEST_POST]-(p1:post)-[:PREV_POST*0..]-(p:post)
                with p, u
                optional match (p)<-[m:COMMENTED|:LIKE|:DISLIKE]-(u1:user)
                WHERE u1.userID <> u.userID and m.activityID < @activityID
                m.dateCreated as dateCreated, p, u1, type(m) as activity, m.activityID as lastActivityID
                return dateCreated, u1, p, activity, lastActivityID
                ORDER BY m.dateCreated
                limit @limit
            */

            string limitActivity = "";
            if (activityId != 0)
            {
                limitActivity = "and m.activityID < " + activityId;
            }
            _client.Connect();
            var listNotification = _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "})-[:LATEST_POST]-(p1:post)-[:PREV_POST*0..]-(p:post)")
                .With("p, u")
                .OptionalMatch("(p)<-[m:COMMENTED|:LIKE|:DISLIKE]-(u1:user)")
                .Where("u1.userID <> u.userID " + limitActivity)
                .With("m.dateCreated as dateCreated, p, u1, type(m) as activity, m.activityID as lastActivityID")
                .Return((dateCreated, u1, p, activity, lastActivityID) => new Notification
                {
                    dateCreated = dateCreated.As<String>(),
                    activity = activity.As<String>(),
                    lastActivityID = lastActivityID.As<Int16>(),
                    user = u1.As<User>(),
                    post = p.As<Post>()
                })
                //.Return<Notification>("distinct m.dateCreated as dateCreated, u1 as user, p as post")
                .OrderByDescending("dateCreated, lastActivityID")
                .Limit(limit)
                .Results.ToList();
            listNotification.RemoveAll(item => item == null);
            return listNotification;
        }

        public void ResetPassword(string email, string newPassword)
        {
            /*
             * MATCH (n:user { email: '@email' })
                SET n.password = @password
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:user { email: '" + email + "' })")
                           .Set("n.password = '" + newPassword + "' RETURN n")
                           .ExecuteWithoutResults();
        }

        public bool EditProfile(User user)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(n:user { userID: " + user.userID + "})")
                           .Set("n.firstName = '" + user.firstName + "'")
                           .Set("n.lastName = '" + user.lastName + "'")
                           .Set("n.address = '" + user.address + "'")
                           .Set("n.gender = '" + user.gender + "'")
                           .Set("n.phoneNumber = '" + user.phoneNumber + "'")
                           .Set("n.dateOfBirth = '" + user.dateOfBirth + "'")
                           .Set("n.password = '" + user.password + "'")
                           .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool EditComment(Comment comment)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(c:comment { commentID: " + comment.commentID + " })")
                       .Set("c.dateCreated = '" + comment.dateCreated + "'")
                       .Set("c.content = '" + comment.content + "'")
                       .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool DeleteComment(int commentId, int userId)
        {
            /*
             * Delete Comment with the following step:
             *      - Find prev comment of this
             *      - Find next comment of this
             *      - Delete relation from prev -> this & this -> next
             *      - Create relation from prev -> next
             *      
             *      - Delete this
             *      - Update the relation(Commented) of user -> post
             */
            _client.Connect();
            try
            {
                var prev = _client.Cypher.OptionalMatch("(c:comment {commentID:" + commentId + "})<-[:PREV_COMMENT*1..1]-(c_prev:comment)")
                    .Return<Comment>("c_prev")
                    .Results.SingleOrDefault();

                var next = _client.Cypher.OptionalMatch("(c:comment {commentID:" + commentId + "})-[:PREV_COMMENT*1..1]->(c_next:comment)")
                    .Return<Comment>("c_next")
                    .Results.SingleOrDefault();

                var post = _client.Cypher.OptionalMatch("(c:comment {commentID:" + commentId + "})-[:PREV_COMMENT*0..]-(c1:comment)-[:LATEST_COMMENT]-(p:post)")
                    .Return<Post>("p")
                    .Results.SingleOrDefault();

                if (prev == null)
                {
                    if (next != null)
                    {
                        if (post != null)
                            _client.Cypher.Match("(c:comment {commentID:" + next.commentID + "}), (p:post {postID: " + post.postID + "})")
                                .Create("p-[:LATEST_COMMENT]->c")
                                .ExecuteWithoutResults();
                    }
                }
                else
                {
                    if (next != null)
                    {
                        _client.Cypher.Match("(c:comment {commentID:" + prev.commentID + "}), (c1:comment {commentID: " + next.commentID + "})")
                                .Create("c-[:PREV_COMMENT]->c1")
                                .ExecuteWithoutResults();
                    }
                }

                _client.Cypher.Match("(c:comment {commentID:" + commentId + "})-[r]-()")
                                .Delete("c,r")
                                .ExecuteWithoutResults();

                if (post != null)
                {
                    var newLast = _client.Cypher.OptionalMatch("(p:post {postID:" + post.postID + "})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)")
                        .Where("(:user {userID:" + userId + "})-[:CREATE]->(c1)")
                        .Return<Comment>("c1")
                        .OrderByDescending("c1.dateCreated")
                        .Results.FirstOrDefault();

                    if (newLast == null)
                    {
                        _client.Cypher.OptionalMatch("(u:user {userID: " + userId + "})-[r:COMMENTED]->(p:post {postID:" + post.postID + "})")
                                     .Delete("r")
                                     .ExecuteWithoutResults();
                    }
                    else
                    {
                        _client.Cypher.OptionalMatch("(u:user {userID: " + userId + "})-[r:COMMENTED]->(p:post {postID:" + post.postID + "})")
                                     .Set("r.dateCreated = '" + newLast.dateCreated + "'")
                                     .ExecuteWithoutResults();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public Message GetLatestMessage(int conversationId)
        {
            _client.Connect();
            Message message;
            try
            {
                message = _client.Cypher.OptionalMatch("(c:conversation { conversationID: " + conversationId + "})-[:LATEST_MESSAGE]-(m:message)")
                                        .Return<Message>("m")
                                        .Results.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            return message;
        }
        public List<Message> GetListMessageInRoom(int roomId)
        {
            _client.Connect();
            List<Message> listMessage = new List<Message>();
            try
            {
                listMessage = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})-[:HAS]->(c:conversation)-[:LATEST_MESSAGE]->(m:message)-[:PREV_MESSAGE*0..]->(m1:message)")
                                        .Return<Message>("m1")
                                        .Results.ToList();
                listMessage.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listMessage = new List<Message>();
            }
            return listMessage;
        }

        public List<Message> GetListMessage(int userId, int friendId, int limit = 10)
        {
            _client.Connect();
            List<Message> listMessage;
            try
            {
                int conversationId = GetConversationId(userId, friendId);

                listMessage = _client.Cypher.OptionalMatch("(c:conversation { conversationID: " + conversationId + "})-[:LATEST_MESSAGE]-(m:message)-[:PREV_MESSAGE*0.." + limit + "]-(m1:message)")
                                        .ReturnDistinct<Message>("m1")
                                        .Results.ToList();
                listMessage.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<Message>();
            }
            return listMessage;
        }

        public User FindUser(Message message)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has message
                 * 
                 * MATCH(u:user)-[:CREATE]->(m:message{messageID:@messageID})
                    return u
                 */
            if (message == null)
            {
                return null;
            }
            _client.Connect();
            var user = _client.Cypher.OptionalMatch("(u:user)-[:CREATE]->(m:message{messageID:" + message.messageID + "})")
                .Return<User>("u")
                .Results.FirstOrDefault();
            return user;
        }

        public Message CreateMessageInRoom(int roomId, int userId, string content)
        {
            _client.Connect();
            
            // Have to make sure conversation is created while creating room.
            var conversation = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})-[:HAS]->(c:conversation)")
                    .Return<Conversation>("c")
                    .Results.First();

            return CreateMessage(content, userId, 0, conversation.conversationID);
        }
        public Message CreateMessage(string content, int userId, int otherId, int conversationId = -1)
        {
            /*
                 * Query:
                 * Find:
                 *     - Create Message
                 *     - If not have Conversation between 2 users:
                 *          + Create Conversation
                 *          + Create Relation (BELONG_TO) from user to Conversation
                 *          + Create Relation (BELONG_TO) from otherUser to Conversation
                 *          + Create Message    
                 *          + Create Relation (LATEST_MESSAGE) from Conversation to Message
                 *          + Create Relation (CREATE) from user to message
                 *       else:     
                 *          + Create Message 
                 *          + Delete Relation from (Conversation) to LatestMessage
                 *          + Create Relation (LATEST_MESSAGE) from Conversation to Message
                 *          + Create Relation (PREV_MESSAGE) from Message to LatestMessage
                 *          + Create Relation (CREATE) from user to message
                 */
            Message message;
            _client.Connect();

            conversationId = conversationId == -1 ? GetConversationId(userId, otherId) : conversationId;

            try
            {
                message = new Message
                {
                    content = content,
                    dateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                    messageID = GetGlobalIncrementId()
                };

                _client.Cypher.Create("(m:message {newMessage})")
                                    .WithParam("newMessage", message)
                                    .ExecuteWithoutResults();

                if (conversationId == -1)
                {
                    var conversation = new Conversation
                    {
                        DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                        conversationID = GetGlobalIncrementId()
                    };

                    _client.Cypher.Create("(c:conversation {newConversation})")
                                        .WithParam("newConversation", conversation)
                                        .ExecuteWithoutResults();

                    _client.Cypher.Match("(c:conversation {conversationID: " + conversationId + "}), (u:user {userID:" + userId + "}), (u1:user {userID:" + otherId + "}), (m:message {messageID:" + message.messageID + "})")
                                        .Create("u-[:BELONG_TO]->c")
                                        .Create("u1-[:BELONG_TO]->c")
                                        .Create("c-[:LATEST_MESSAGE]->m")
                                        .Create("u-[:CREATE]->m")
                                        .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.OptionalMatch("(c:conversation {conversationID: " + conversationId + "})-[r:LATEST_MESSAGE]->(m1:message), (u:user {userID:" + userId + "}), (m:message {messageID:" + message.messageID + "})")
                                        .Delete("r")
                                        .Create("c-[:LATEST_MESSAGE]->m")
                                        .Create("m-[:PREV_MESSAGE]->m1")
                                        .Create("u-[:CREATE]->m")
                                        .ExecuteWithoutResults();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            return message;
        }

        public int GetConversationId(int userId, int otherId)
        {
            _client.Connect();

            var conversationIdList = _client.Cypher.Match("(u1:user{userID: " + userId +
                                 "})-[:BELONG_TO]->(c:conversation)<-[:BELONG_TO]-(u2:user{userID: " + otherId + "})")
                .ReturnDistinct<int>("c.conversationID")
                .Results
                .ToList();

            return conversationIdList.Any() ? conversationIdList.First() : -1;
        }

        public bool SendRequestFriend(int userId, int otherUserId)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(u:user {userID:" + userId + "}), (u1:user {userID:" + otherUserId + "})")
                                    .Create("u-[:FRIEND_REQUEST {dateCreated: '" + DateTime.Now.ToString(FapConstants.DatetimeFormat) + "'}]->u1")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool DeclineRequestFriend(int userId, int otherUserId)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:FRIEND_REQUEST]-(u1:user {userID:" + otherUserId + "})")
                                    .Delete("r")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool AddFriend(int userId, int otherUserId)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(u:user {userID:" + userId + "}), (u1:user {userID:" + otherUserId + "})")
                                    .Create("u-[:FRIEND]->u1")
                                    .Create("u1-[:FRIEND]->u")
                                    .ExecuteWithoutResults();

                _client.Cypher.OptionalMatch("(u:user {userID:" + userId + "})-[r:FRIEND_REQUEST]-(u1:user {userID:" + otherUserId + "})")
                                    .Delete("r")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public bool Unfriend(int userId, int otherUserId)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(u:user {userID:" + userId + "})-[r:FRIEND]-(u1:user {userID:" + otherUserId + "})")
                                    .Delete("r")
                                    .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public List<User> GetListFriend(int userId)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * match(u1:user{userId:@userId})<-[m:FRIEND_REQUEST]-(u2:user)
                    return u2;
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u1:user {userID:" + userId + "})<-[:FRIEND_REQUEST]-(u2:user)")
                .ReturnDistinct<User>("u2")
                .Results.ToList();
            listUser.RemoveAll(item => item == null);
            return listUser;
        }

        public bool DeletePost(int postId)
        {
            _client.Connect();
            User user = SearchUser(postId);

            DeleteRelatedPhotosAndRelationships(postId, user.userID);
            DeletePlaceRelationship(postId);

            try
            {
                //TODO: Remove likes, dislike
                DeleteCommentsAndRelationship(postId);
                RebuildPostsChainFlow(user.userID, postId);
                DeletePostAndRelationships(postId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
        #region DELETE POST HELPERS

        private void DeleteCommentsAndRelationship(int postId)
        {
            _client.Cypher.Match("(post{postID: " + postId + "})-[lcr:LATEST_COMMENT]->(lc:comment)-[pcrs:PREV_COMMENT*]->(pc:comment), ()-[ccrl:CREATE]->lc,()-[ccrp:CREATE]->pc")
                          .ForEach("(pcr IN pcrs| DELETE pcr)")
                         .Delete("lcr, ccrl, ccrp, lc, pc")
                         .ExecuteWithoutResults();
        }

        private void DeletePostAndRelationships(int postId)
        {
            _client.Cypher.Match("(p:post {postID:" + postId + "})-[r]-()")
                .Delete("p,r")
                .ExecuteWithoutResults();
        }

        private void RebuildPostsChainFlow(int userId, int postId)
        {
            var prev = FindPrevPost(postId);
            var next = FindNextPost(postId);

            if (prev == null)
            {
                if (next != null)
                {
                    _client.Cypher.Match("(u:user {userID:" + userId + "}), (p:post {postID: " + next.postID + "})")
                        .Create("(u)-[:LATEST_POST]->(p)")
                        .ExecuteWithoutResults();
                }
            }
            else
            {
                if (next != null)
                {
                    _client.Cypher.Match("(pp:post {postID:" + prev.postID + "}), (pn:post {postID: " + next.postID + "})")
                        .Create("pp-[:PREV_POST]->pn")
                        .ExecuteWithoutResults();
                }
            }
        }

        private Post FindNextPost(int postId)
        {
            return _client.Cypher.OptionalMatch("(p:post {postID:" + postId + "})-[:PREV_POST*1..1]->(p_next:post)")
                .Return<Post>("p_next")
                .Results.SingleOrDefault();
        }

        private Post FindPrevPost(int postId)
        {
            return _client.Cypher.OptionalMatch("(p:post {postID:" + postId + "})<-[:PREV_POST*1..1]-(p_prev:post)")
                .Return<Post>("p_prev")
                .Results.SingleOrDefault();
        }

        private void DeletePlaceRelationship(int postId)
        {
            _client.Cypher.Match("(post{postID: " + postId + "})-[r1:AT]->(place)<-[r2:VISITED]-(user)")
                         .Delete("r1, r2")
                         .ExecuteWithoutResults();
        }

        private void DeleteRelatedPhotosAndRelationships(int postId, int userId)
        {
            _client.Cypher.OptionalMatch("(p:Post {postID: " + postId + "})-[r1:HAS]-(pt:Photo)<-[r2:HAS]-(u:User {userID: " + userId + "})")
                .Delete("r1, r2, pt")
                .ExecuteWithoutResults();
        }
        #endregion

        public bool EditPost(int postId, string newContent)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(p:post { postID: " + postId + " })")
                       .Set("p.dateCreated = '" + DateTime.Now.ToString(FapConstants.DatetimeFormat) + "'")
                       .Set("p.content = '" + newContent + "'")
                       .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public List<User> SearchUserByKeyword(string keyword)
        {
            List<User> listUser;
            /*
             * match (u:user)
                where upper(u.firstName + ' ' + u.lastName) =~ '.*@keyword.*'
                return u
             */
            _client.Connect();
            try
            {
                listUser = _client.Cypher
                       .Match("(u:user)")
                       .Where("upper(u.firstName + ' ' + u.lastName) =~ '.*" + keyword + ".*'")
                       .ReturnDistinct<User>("u")
                       .Results.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listUser = new List<User>();
            }

            listUser.RemoveAll(item => item == null);
            return listUser;
        }

        public List<Place> SearchPlaceByKeyword(string keyword)
        {
            List<Place> listPlace;
            /*
             * match (p:place)
                where upper(p.name) =~ '.*@keyword.*'
                return p
             */
            _client.Connect();
            try
            {
                listPlace = _client.Cypher
                   .Match("(p:place)")
                   .Where("upper(p.name) =~ '.*" + keyword + ".*'")
                   .ReturnDistinct<Place>("p")
                   .Results.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listPlace = new List<Place>();
            }

            listPlace.RemoveAll(item => item == null);
            return listPlace;
        }

        public List<Room> SearchRoomByKeyword(string keyword)
        {
            List<Room> listRoom;
            /*
             * match (p:room)
                where upper(p.RoomName) =~ '.*@keyword.*'
                return p
             */
            _client.Connect();
            try
            {
                listRoom = _client.Cypher
                   .Match("(r:room)")
                   .Where("upper(r.RoomName) =~ '.*" + keyword + ".*'")
                   .ReturnDistinct<Room>("r")
                   .Results.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listRoom = new List<Room>();
            }

            listRoom.RemoveAll(item => item == null);
            return listRoom;
        }

        public List<Photo> SearchPhotoInPlace(int placeId)
        {
            /*
             * match(pl:place {placeID:@placeID})<-[:AT]-(p:post)
                with pl, p
                optional match (p)<-[:LIKE]-(u:user)
                with p, COUNT(Distinct u) as number
                match (ph:photo)<-[:HAS]-(p)
                return ph, number
                ORDER BY number DESC
             */
            List<Photo> listPhoto;
            _client.Connect();
            try
            {
                listPhoto = _client.Cypher
                   .Match("(pl:place {placeID:" + placeId + "})<-[:AT]-(p:post)")
                   .With("pl, p")
                   .OptionalMatch("(p)<-[:LIKE]-(u:user)")
                   .With("p, COUNT(Distinct u) as number")
                   .Match("(ph:photo)<-[:HAS]-(p)")
                   .With("ph, number")
                   .OrderByDescending("number")
                   .Limit(4)
                   .ReturnDistinct<Photo>("ph")
                   .Results.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listPhoto = new List<Photo>();
            }

            listPhoto.RemoveAll(item => item == null);
            return listPhoto;
        }

        public int CountPostAtPlace(int placeId)
        {
            /*
             * optional match (pl:place {placeID:@placeID})<-[:AT]-(p:post)
                return COUNT(p)
             */
            int numberOfPost;
            _client.Connect();
            try
            {
                numberOfPost = _client.Cypher
                   .OptionalMatch("(pl:place {placeID:" + placeId + "})<-[:AT]-(p:post)")
                   .ReturnDistinct<int>("COUNT (p)")
                   .Results.First();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                numberOfPost = 0;
            }
            return numberOfPost;
        }


        public bool CreateNewPlanEvent(Plan newPlan, int roomId, int userId)
        {
            newPlan.PlanId = GetActivityIncrementId();
            _client.Connect();

            _client.Cypher
                   .Create("(plan:plan {newPlan})")
                   .WithParam("newPlan", newPlan)
                   .ExecuteWithoutResults();

            _client.Cypher.Match("(r:room {RoomId:" + roomId + "}), (p:plan {PlanId: " + newPlan.PlanId + "})")
                         .Create("(r)-[r:HAS]->(p)")
                         .ExecuteWithoutResults();

            _client.Cypher.Match("(u:user {userID: " + userId + "}), (p:plan {PlanId: " + newPlan.PlanId + "})")
                         .Create("(u)-[r:CREATE]->(p)")
                         .ExecuteWithoutResults();

            return true;
        }

        public List<Plan> LoadAllPlansInDateRange(DateTime fromDate, DateTime toDate)
        {
            //DateTime fromDate = DateHelpers.Instance.ConvertFromUnixTimestamp(start) ?? DateTime.MinValue;

            List<Plan> listPlan;
            _client.Connect();
            try
            {
                listPlan = _client.Cypher
                       .Match("(p:plan)")
                       .ReturnDistinct<Plan>("p")
                       .Results.ToList()
                       .Where(p => DateTime.ParseExact(p.DatePlanStart, FapConstants.DatetimeFormat, CultureInfo.InvariantCulture) >= fromDate
                                   && DateTime.ParseExact(p.DatePlanStart, FapConstants.DatetimeFormat, CultureInfo.InvariantCulture).AddMinutes(p.LengthInMinute) <= toDate)
                       .ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listPlan = new List<Plan>();
            }

            return listPlan;
        }

        /*
        public IEnumerable<object> LoadPlansSummaryInDateRange(double start, double end)
        {
            DateTime fromDate = DateTime.MinValue;
            DateTime toDate = DateTime.MaxValue;

            IEnumerable<object> listPlan;
            _client.Connect();
            try
            {
                listPlan = _client.Cypher
                       .Match("(p:plan)")
                       .ReturnDistinct<Plan>("p")
                       .Results.ToList()
                       .Where(p => DateTime.ParseExact(p.DatePlanStart, FapConstants.DatetimeFormat, CultureInfo.InvariantCulture) >= fromDate
                                   && DateTime.ParseExact(p.DatePlanStart, FapConstants.DatetimeFormat, CultureInfo.InvariantCulture).AddMinutes(p.LengthInMinute) <= toDate)
                       .GroupBy(p => DateTime.ParseExact(p.DatePlanStart, FapConstants.DatetimeFormat, CultureInfo.InvariantCulture).Date)
                       .Select(x => new { DateTimeScheduled = x.Key, Count = x.Count() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                listPlan = new List<Plan>();
            }

            return listPlan;
        }
         */

        public List<Post> FindPostInRoom(int roomId, int postId, int limit = 5)
        {
            /*
                 * Query:
                 * Find:
                 *     - List Post in room
                 * 
                 */
            _client.Connect();
            List<Post> listPost = new List<Post>();
            try
            {
                listPost = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})-[l:LATEST_POST]->(p1:post)-[pr:PREV_POST*0..]->(p2:post)")
                    //.Where("p2.id < " + postID)
                    .ReturnDistinct<Post>("p2")
                    .Limit(limit)
                    .Results.ToList();

                listPost.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return listPost;
            }
            return listPost;
        }

        public List<User> FindUserInRoom(int roomId)
        {
            /*
                 * Query:
                 * Find:
                 *     - List User in room
                 * 
                 */
            _client.Connect();
            List<User> listUser = new List<User>();
            try
            {
                listUser = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})<-[j:JOIN]-(u:user)")
                    .ReturnDistinct<User>("u")
                    .Results.ToList();

                listUser.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return listUser;
            }
            return listUser;
        }

        public User FindAdminInRoom(int roomId)
        {
            /*
                 * Query:
                 * Find:
                 *     - List User in room
                 * 
                 */
            _client.Connect();
            User admin = new User();
            try
            {
                admin = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})<-[j:JOIN {type:" + FapConstants.JOIN_ADMIN + "}]-(u:user)")
                    .ReturnDistinct<User>("u")
                    .Results.FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return admin;
            }
            return admin;
        }

        public List<User> FindUserRequestJoinRoom(int roomId)
        {
            /*
                 * Query:
                 * Find:
                 *     - List User in room
                 * 
                 */
            _client.Connect();
            List<User> listUser = new List<User>();
            try
            {
                listUser = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})<-[j:JOIN {type:" + FapConstants.JOIN_REQUEST + "}]-(u:user)")
                    .ReturnDistinct<User>("u")
                    .Results.ToList();

                listUser.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return listUser;
            }
            return listUser;
        }

        public List<Message> GetListMessageInRoom(int roomId, int messageId)
        {
            _client.Connect();
            List<Message> listMessage;
            try
            {
                listMessage = _client.Cypher.OptionalMatch("(r:room {RoomId: " + roomId + "})-[:HAS]->(c:conversation)-[:LATEST_MESSAGE]->(m:message)-[:PREV_MESSAGE*0..]->(m1:message)")
                    //.Where("p1.messageID < " + messageID)
                                        .ReturnDistinct<Message>("m1")
                                        .Results.Reverse().ToList();
                listMessage.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<Message>();
            }
            return listMessage;
        }

        public List<Post> FindPostInPlace(int placeID, int postID = 0, int limit = 5)
        {
            /*
                 * Query:
                 * Find:
                 *     optional match (pl:place {placeID:@placeID})<-[:AT]-(p:post)
                        optional match (p)<-[:LIKE]-(ul:user)
                        optional match (p)<-[:DISLIKE]-(udl:user)
                        optional match (p)-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)
                        with p, (COUNT(DISTINCT ul)) - (COUNT(DISTINCT udl)) + (COUNT(DISTINCT c1)) as mark
                        return p
                        ORDER BY mark
                 * 
                 */
            _client.Connect();
            List<Post> listPost = new List<Post>();
            try
            {
                listPost = _client.Cypher.OptionalMatch("(pl:place {placeID:" + placeID + "})<-[:AT]-(p:post)")
                                        .OptionalMatch("(p)<-[:LIKE]-(ul:user)")
                                        .OptionalMatch("(p)<-[:DISLIKE]-(udl:user)")
                                        .OptionalMatch("(p)-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)")
                                        .With("p, (COUNT(DISTINCT ul)) - (COUNT(DISTINCT udl)) + (COUNT(DISTINCT c1)) as mark")
                                        .OrderByDescending("mark")
                                        .Return<Post>("p")
                    //.Where("p2.id < " + postID)
                    //.Limit(limit)
                                        .Results.ToList();

                listPost.RemoveAll(item => item == null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return listPost;
            }
            return listPost;
        }

        public Place FindPlaceByID(int placeID)
        {

            /*
                 * Query:
                 * Find:
                 *     - Find Place by id
                 */

            _client.Connect();
            Place place = null;
            try
            {
                place = _client.Cypher.OptionalMatch("(pl:place {placeID:" + placeID + "})")
                    .Return<Place>("pl")
                    .Results
                    .FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                place = null;
            }
            return place;
        }
        public bool UpdatePlanEvent(string id, DateTime newEventStart, DateTime newEventEnd)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(p:plan { PlanId: " + id + "})")
                           .Set("p.DatePlanStart = '" + newEventStart.ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture) + "'")
                           .Set("p.LengthInMinute = '" + (newEventEnd - newEventStart).TotalMinutes + "'")
                           .ExecuteWithoutResults();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        public void InsertRoom(Room newRoom, int currentUserId)
        {
            // Auto increment Id.
            newRoom.RoomId = GetGlobalIncrementId();
            _client.Connect();

            _client.Cypher
                   .Create("(r:room {newRoom})")
                   .WithParam("newRoom", newRoom)
                   .ExecuteWithoutResults();

            var newConversation = new Conversation
            {
                conversationID = GetGlobalIncrementId(),
                DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture)
            };

            _client.Cypher.Match("(u:user {userID:" + currentUserId + "}), (r:room {RoomId: " + newRoom.RoomId + "})")
                     .Create("(u)-[:JOIN {type: " + FapConstants.JOIN_ADMIN + "}]->(r)")
                     .ExecuteWithoutResults();

            /*
            _client.Cypher
                   .Create("(c:conversation {newConversation})")
                   .WithParam("newConversation", newConversation)
                   .ExecuteWithoutResults();

            _client.Cypher.Match("(r:room {RoomId: " + newRoom.RoomId + "}), (c:conversation {conversationID: " + newConversation.conversationID + "})")
                     .Create("(r)-[:HAS]->(c)")
                     .ExecuteWithoutResults();
             */
        }

        public Room GetRoomInformation(int roomId)
        {
            _client.Connect();
            return _client.Cypher.Match("(r:room{RoomId: " + roomId + "})")
                .Return<Room>("r")
                .Results
                .FirstOrDefault();
        }
    }
}