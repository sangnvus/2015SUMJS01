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

        public class ReportPost
        {
            public string postContent { get; set; }
            public int postID { get; set; }
            public int numberReport { get; set; }
        }

        public class ReportUser
        {
            public string userReportedName { get; set; }
            public int userReportedID { get; set; }
            public int numberReport { get; set; }
        }

        public string GetEmailByUserId(int userId)
        {
            _client.Connect();
            string email = _client.Cypher.Match("(u:user {UserId:" + userId + "})")
                                    .Return<string>("u.Email")
                                    .Results.Single();
            return email;
        }

        public void LockUser(int userId)
        {
            /*
             * MATCH (n:user { UserId: userId })
                SET n.status = 'lock'
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:user { UserId: " + userId + " })")
                           .Set("n.Status = 'lock'")
                           .ExecuteWithoutResults();
        }

        public void LockPost(int postId)
        {
            /*
             * MATCH (n:post { PostId: postId })
                SET n.status = 'lock'
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:post { PostId: " + postId + " })")
                           .Set("n.Privacy = 'lock' RETURN n")
                           .ExecuteWithoutResults();
        }

        public void UnlockUser(int userId)
        {
            /*
             * MATCH (n:user { UserId: userId })
                SET n.status = 'lock'
                RETURN n
             */
            _client.Connect();
            _client.Cypher.Match("(n:user { UserId: " + userId + " })")
                           .Set("n.Status = 'active' RETURN n")
                           .ExecuteWithoutResults();
        }

        public void InsertUser(ref User user)
        {
            // Auto increment Id.
            user.UserId = GetGlobalIncrementId();
            _client.Connect();

            _client.Cypher
                   .Create("(user:user {newUser})")
                   .WithParam("newUser", user)
                   .ExecuteWithoutResults();
        }

        public void InsertReportPost(int postId, int userReportId, int typeReport)
        {
            string ContentReport = null;
            if (typeReport == 1)
            {
                ContentReport = "This post annoying or unpleasant";
            }
            else if (typeReport == 2)
            {
                ContentReport = "I think this post should not appear on FlyAwayPlus";
            }
            else if (typeReport == 3)
            {
                ContentReport = "This post is spam";
            }
            int count = _client.Cypher.Match("(u:user{UserId:" + userReportId + "})-[r:REPORT_POST]->(p:post{PostId:" + postId + "})")
                                   .Return<int>("COUNT(r)")
                                   .Results.Single();
            if (count == 0)
            {
                _client.Cypher.Match("(a:user), (b:post)")
                                 .Where("a.UserId = " + userReportId + " AND b.PostId = " + postId)
                                 .Create("(a)-[r:REPORT_POST {ContentReport: '" + ContentReport + "'}]->(b)")
                                 .ExecuteWithoutResults();
            }
        }

        public void InsertReportUser(int userReportId, int userReportedId, int typeReport)
        {
            string ContentReport = null;
            if (typeReport == 1)
            {
                ContentReport = "This user annoying or unpleasant";
            }
            else if (typeReport == 2)
            {
                ContentReport = "I think this user should not appear on FlyAwayPlus";
            }
            else if (typeReport == 3)
            {
                ContentReport = "This user is spam";
            }
            int count = _client.Cypher.Match("(u1:user{UserId:" + userReportId + "})-[r:REPORT_USER]->(u2:user{UserId:" + userReportedId + "})")
                                   .Return<int>("COUNT(r)")
                                   .Results.Single();
            if (count == 0)
            {
                _client.Cypher.Match("(a:user), (b:user)")
                                 .Where("a.UserId = " + userReportId + " AND b.UserId = " + userReportedId)
                                 .Create("(a)-[r:REPORT_USER {ContentReport: '" + ContentReport + "'}]->(b)")
                                 .ExecuteWithoutResults();
            }
        }

        public bool IsLike(int postId, int userId)
        {
            _client.Connect();
            int like = _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:LIKE]->(p:post {PostId:" + postId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return like != 0;
        }

        public bool IsFriend(int userId, int otherUserId)
        {
            _client.Connect();
            int friend = _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:FRIEND]->(u1: user{UserId: " + otherUserId + "})")
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
            int friend = _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:FRIEND_REQUEST]->(u1: user{UserId: " + otherUserId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return friend != 0 ? "request" : "none";
        }

        public bool IsDislike(int postId, int userId)
        {
            // Auto increment Id

            _client.Connect();
            int dislike = _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:DISLIKE]->(p:post {PostId:" + postId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return dislike != 0;
        }

        public bool IsWish(int postId, int userId)
        {
            _client.Connect();
            int wish = _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:WISH]->(p:post {PostId:" + postId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return wish != 0;
        }

        public bool IsInWishist(int placeId, int userId)
        {
            _client.Connect();
            int wish = _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "})-[r:WISH]->(p:place {PlaceId:" + placeId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.FirstOrDefault();

            return wish != 0;
        }

        public bool AddToWishlist(int placeId, int userId)
        {
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {UserId:" + userId + "}), (p:place {PlaceId:" + placeId + "})")
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
                _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:WISH]->(p:place {PlaceId:" + placeId + "})")
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
                _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:WISH]->(p:post {PostId:" + postId + "})")
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
             * Match (p:post {PostId:@PostId})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)
                return COUNT (DISTINCT u)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {PostId:" + postId + "})<-[r:COMMENTED]-(u:user)")
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
             * match(p:post {PostId:@PostId})-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)
                    return Length(collect(c1)) as CommentNumber
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {PostId:" + postId + "})-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)")
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
             * Match (p:post {PostId:@PostId})<-[r:DISLIKE]-(u:user)
                return COUNT(DISTINCT c)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {PostId:" + postId + "})<-[r:DISLIKE]-(u:user)")
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
             * Match (p:post {PostId:@PostId})<-[r:LIKE]-(u:user)
                return COUNT(DISTINCT r)
             */
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(p:post {PostId:" + postId + "})<-[r:LIKE]-(u:user)")
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
             * optional match (u:user {UserId:@UserId})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user{UserId:@otherUserID})
                    With count(DISTINCT mf) AS mutualFriends
                    RETURN mutualFriends
             */
            try
            {
                _client.Connect();
                return userId == otherUserId
                    ? CountFriends(userId)
                    : _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user{UserId:" + otherUserId + "})")
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

        public int CountFriends(int userId)
        {
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(u:user {UserId:" + userId + "})<-[:FRIEND]-(other:user)")
                                .Return<int>("COUNT(DISTINCT other)")
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
             * Match (p:post {PostId:@PostId})-[r:has]->(c:comment), (u:user)-[r1:has]-(c)
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

        public int CountPlaceOfUser(int userId)
        {
            try
            {
                _client.Connect();
                return _client.Cypher.Match("(:user{UserId: " + userId + "})-[:VISITED]->(p:place)")
                                .Return<int>("COUNT(distinct p)")
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
                return _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:LIKE]->(p:post {PostId:" + postId + "})")
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
                return _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:DISLIKE]->(p:post {PostId:" + postId + "})")
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
             * Match(u:user {UserId:@UserId})-[r:like]->(p:post {PostId:@PostId})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:LIKE]->(p:post {PostId:" + postId + "})")
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
             * Match(u:user {UserId:@UserId})-[r:dislike]->(p:post {PostId:@PostId})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:DISLIKE]->(p:post {PostId:" + postId + "})")
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

        public bool DeleteReportPost(int postId)
        {
            /**
             * Match(u:user {UserId:@UserId})-[r:dislike]->(p:post {PostId:@PostId})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u:user)-[r:REPORT_POST]-(p:post {PostId:" + postId + "})")
                                .Delete("r")
                                .ExecuteWithoutResults();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool DeleteReportUser(int userReportedId)
        {
            /**
             * Match(u:user {UserId:@UserId})-[r:dislike]->(p:post {PostId:@PostId})
                delete r
             */
            try
            {
                _client.Connect();
                _client.Cypher.Match("(u1:user)-[r:REPORT_USER]-(u2:user {UserId:" + userReportedId + "})")
                                .Delete("r")
                                .ExecuteWithoutResults();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool InsertDislike(int userId, int postId)
        {
            Node<User> userNode = GetNodeUser(userId);

            NodeReference<Post> postRef = _client.Cypher.Match("(p:post {PostId:" + postId + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                _client.CreateRelationship(userRef, new UserDislikePostRelationship(postRef, new { DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat), ActivtyId = GetActivityIncrementId() }));
                return true;
            }
            return false;
        }

        public bool InsertLike(int userId, int postId)
        {
            Node<User> userNode = GetNodeUser(userId);

            NodeReference<Post> postRef = _client.Cypher.Match("(p:post {PostId:" + postId + "})")
                                            .Return<Node<Post>>("p")
                                            .Results.Single()
                                            .Reference;
            if (userNode != null)
            {
                var userRef = userNode.Reference;
                _client.CreateRelationship(userRef, new UserLikePostRelationship(postRef, new { DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat), ActivtyId = GetActivityIncrementId() }));
                return true;
            }
            return false;
        }

        public Node<User> GetNodeUser(int id)
        {
            _client.Connect();
            return _client.Cypher.Match("(u:user {UserId: " + id + "})").Return<Node<User>>("u").Results.FirstOrDefault();
        }

        public Node<Post> GetNodePost(int id)
        {
            _client.Connect();
            return _client.Cypher.Match("(p:post {PostId: " + id + "})").Return<Node<Post>>("p").Results.FirstOrDefault();
        }

        public User GetUser(int typeId, string email)
        {
            _client.Connect();
            var user = _client.Cypher.OptionalMatch("(u:user {TypeId:" + typeId + ", Email: '" + email + "'})")
                .Return<User>("u")
                .Results.FirstOrDefault();
            return user;
        }

        public User GetUser(int userId)
        {
            _client.Connect();
            var user = _client.Cypher.Match("(u:user {UserId:" + userId + "})")
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
                 * MATCH(u:user)-[:has]->(c:comment{CommentId:@CommentId})
                    return u
                 */
            _client.Connect();
            var user = _client.Cypher.Match("(u:user)-[:CREATE]->(c:comment{CommentId:" + comment.CommentId + "})")
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
                 * MATCH(u:user {UserId:@UserId})
                    return u
                 */
            User user;
            try
            {
                _client.Connect();
                user = _client.Cypher.Match("(u:user {UserId:" + id + "})")
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
                 * MATCH(u:user {UserId:@userEmail})
                    return u
                 */
            User user;
            try
            {
                _client.Connect();
                user = _client.Cypher.Match("(u:user {Email:'" + email + "'})")
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
            var listAllReportPosts = _client.Cypher.OptionalMatch("(u:user)-[r:REPORT_POST]->(p:post)")
                .With("p.PostId as postId, p.Content as postContent, COUNT(r) as numberReport")
                .OrderByDescending("numberReport")
                .Return((postId, postContent, numberReport) => new ReportPost
                {
                    postContent = postContent.As<String>(),
                    postID = postId.As<Int16>(),
                    numberReport = numberReport.As<Int16>()
                })
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
            var listAllReportUsers = _client.Cypher.OptionalMatch("(u1:user)-[r:REPORT_USER]->(u2:user)")
                 .With("u2.UserId as userReportedId, u2.FirstName + ' ' + u2.LastName as userReportedName, COUNT(r) as numberReport")
                 .OrderByDescending("numberReport")
                 .Return((userReportedId, userReportedName, numberReport) => new ReportUser
                 {
                     userReportedName = userReportedName.As<String>(),
                     userReportedID = userReportedId.As<Int16>(),
                     numberReport = numberReport.As<Int16>()
                 })
                 .Results.ToList();
            return listAllReportUsers;
        }

        public List<User> FindFriend(User user)
        {

            /*
                 * Query:
                 * Find:
                 *     - all friend of current user
                 * 
                 * match(u1:user{UserId:@UserId})-[m:friend]->(u2:user)
                    return u2;
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u1:user {UserId:" + user.UserId + "})-[:FRIEND]->(u2:user)")
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
                 * optional match (u:user {UserId:@UserId})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user)
                    WHERE NOT(u-[:FRIEND]->other)
                    WITH other,count(DISTINCT mf) AS mutualFriends
                    ORDER BY mutualFriends DESC
                    LIMIT @limit
                    RETURN other
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "})-[:FRIEND]->(mf:user)<-[:FRIEND]-(other:user)")
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
            int relation = _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:VISITED]->(p:place {PlaceId:" + placeId + "})")
                                    .Return<int>("COUNT(r)")
                                    .Results.Single();

            return relation != 0;
        }

        public int NumberOfPost(int placeId)
        {
            // Auto increment Id

            _client.Connect();
            int count = _client.Cypher.Match("(po:post)-[r:AT]->(pl:place {PlaceId:" + placeId + "})")
                                   .Return<int>("COUNT(Distinct po)")
                                   .Results.Single();

            return count;
        }

        public int NumberOfUser()
        {
            // Auto increment Id

            _client.Connect();
            int count = _client.Cypher.Match("(u:user)")
                                   .Return<int>("COUNT(u)")
                                   .Results.Single();

            return count;
        }

        public int NumberOfReportPost()
        {
            // Auto increment Id

            _client.Connect();
            int count = _client.Cypher.Match("(u:user)-[r:REPORT_POST]->(p:post)")
                                   .Return<int>("COUNT(r)")
                                   .Results.Single();

            return count;
        }

        public int NumberOfReportUser()
        {
            // Auto increment Id

            _client.Connect();
            int count = _client.Cypher.Match("(u1:user)-[r:REPORT_USER]->(u2:user)")
                                   .Return<int>("COUNT(r)")
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
                 * optional match (u:user {UserId:@UserId})-[:FRIEND]->(u1:user), (other:user)
                    WHERE NOT(u-[:FRIEND]->other) AND NOT(u1-[:FRIEND]->other)
                    RETURN other
                    limit @limit
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "})-[:FRIEND]->(u1:user), (other:user)")
                .Where("NOT(u-[:FRIEND]->other) AND NOT(u1-[:FRIEND]->other)")
                .ReturnDistinct<User>("other")
                .Limit(limit)
                .Results.ToList();
            listUser.RemoveAll(item => item == null);

            if (listUser.Count == 0)
            {
                listUser = _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "}), (other:user)")
                            .Where("u.UserId <> other.UserId")
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
                 * match(u1:user{UserId:@UserId})-[m:friend]->(u2:user)
                    return u2;
                 */
            _client.Connect();
            var listUser = _client.Cypher.OptionalMatch("(u1:user {UserId:" + userId + "})-[:FRIEND]-(u2:user)")
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
                 * Optional Match (u1:user {userID:@userID})-[:WISH]->(pl:place)
                    Optional Match (u2:user)-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)-[:AT]->(pl)
                    Where p1.privacy = 'public' or (p1.privacy = 'friend' and u1-[:FRIEND]-u2) or (p1.privacy <> 'lock' and u1.userID = u2.userID)
                    Return Distinct p1
                   orderby p1.dateCreated
                 */
            _client.Connect();
            var listPost = _client.Cypher.OptionalMatch("(u1:user {UserId:" + user.UserId + "})-[:WISH]->(pl:place)")
                .OptionalMatch("(u2:user)-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)-[:AT]->(pl)")
                .Where("p1.Privacy = 'public' or (p1.Privacy = 'friend' and u1-[:FRIEND]-u2) or (p1.Privacy <> 'lock' and u1.UserId = u2.UserId)")
                .ReturnDistinct<Post>("p1")
                .OrderByDescending("p1.DateCreated")
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

            if (video != null)
            {
                _client.Cypher.Create("(v:video {newVideo})")
                         .WithParam("newVideo", video)
                         .ExecuteWithoutResults();
            }

            Node<User> userNode = GetNodeUser(user.UserId);
            if (userNode != null)
            {
                var existingPlace = FindExistingPlace(place);

                if (existingPlace == null)
                {
                    _client.Cypher.Create("(p:place {newPlace})")
                            .WithParam("newPlace", place)
                            .ExecuteWithoutResults();

                    _client.Cypher.Match("(po:post {PostId:" + post.PostId + "}), (pl:place {PlaceId: " + place.PlaceId + "})")
                             .Create("(po)-[r:AT]->(pl)")
                             .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.Match("(po:post {PostId:" + post.PostId + "}), (pl:place {PlaceId: " + existingPlace.PlaceId + "})")
                                 .Create("(po)-[r:AT]->(pl)")
                                 .ExecuteWithoutResults();
                }

                if (video != null)
                {
                    _client.Cypher.Match("(po:post {PostId:" + post.PostId + "}), (vi:video {VideoId: " + video.VideoId + "})")
                             .Create("(po)-[r:HAS]->(vi)")
                             .ExecuteWithoutResults();
                }

                foreach (var photo in photos)
                {
                    _client.Cypher.Match("(po:post {PostId:" + post.PostId + "}), (pt:photo {PhotoId: " + photo.PhotoId + "})")
                                 .Create("(po)-[r:HAS]->(pt)")
                                 .ExecuteWithoutResults();

                    _client.Cypher.Match("(u:user {UserId:" + user.UserId + "}), (p:photo {PhotoId: " + photo.PhotoId + "})")
                                 .Create("(u)-[r:HAS]->(p)")
                                 .ExecuteWithoutResults();
                }

                _client.Cypher.Match("(u:user {UserId:" + user.UserId + "}), (p:place {PlaceId: " + place.PlaceId + "})")
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
                int oldPost = _client.Cypher.Match("(u:user{UserId:" + user.UserId + "})-[:LATEST_POST]->(p:post)")
                                    .Return<int>("COUNT (p)")
                                    .Results.Single();

                if (oldPost == 0)
                {
                    // CREATE New LATEST_POST
                    _client.Cypher.Match("(u:user{UserId:" + user.UserId + "}), (p1:post{PostId:" + post.PostId + "})")
                                    .Create("(u)-[:LATEST_POST]->(p1)")
                                    .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.Match("(u:user{UserId:" + user.UserId + "})-[r:LATEST_POST]->(p:post), (p1:post{PostId:" + post.PostId + "})")
                                    .Delete("r")
                                    .Create("(u)-[:LATEST_POST]->(p1)")
                                    .Create("(p1)-[:PREV_POST]->(p)")
                                    .ExecuteWithoutResults();
                }
            }
        }

        private void SpecifyIds(ref Post post, ref List<Photo> photos, ref Place place, ref Video video)
        {
            post.PostId = GetGlobalIncrementId();
            foreach (var photo in photos)
            {
                photo.PhotoId = GetGlobalIncrementId();
            }

            place.PlaceId = GetGlobalIncrementId();
            if (video != null)
            {
                video.VideoId = GetGlobalIncrementId();
            }
        }

        public User SearchUser(int postId)
        {
            /**
             * match (p:post{PostId:1003})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                return u
             */
            _client.Connect();
            var user = _client.Cypher.Match("(p:post{PostId:" + postId + "})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)")
                .ReturnDistinct<User>("u")
                .Results.Single();
            return user;
        }

        public List<Comment> FindComment(int postId)
        {
            /*
                 * Query:
                 * Find:
                 *     - find comment of post
                 * 
                 * match (p:post{PostId:@PostId})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)
                    return c1
                 */
            _client.Connect();
            var list = _client.Cypher.Match("(p:post{PostId:" + postId + "})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)")
                .Return<Comment>("c1")
                .OrderBy("c1.DateCreated")
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
                 * match (u:user{UserId:@UserId})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)
                    return p1
                 */
            _client.Connect();
            var listPost = _client.Cypher.OptionalMatch("(u:user {UserId:" + user.UserId + "})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post)")
                .Where("p1.Privacy <> 'lock'")
                .ReturnDistinct<Post>("p1")
                //.OrderByDescending("p.DateCreated")
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public Post FindPostById(int postId)
        {
            /*
                 * Query:
                 * Find:
                 */
            _client.Connect();
            var post = _client.Cypher.Match("(p:post {PostId:" + postId + "})")
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
                 * match (po:post {PostId:@PostId})-[:has]->(ph:photo)
                    return ph
                 */

            _client.Connect();
            return _client.Cypher.Match("(po:post {PostId:" + postId + "})-[:HAS]->(ph:photo)")
                            .Return<Photo>("ph")
                            .OrderBy("ph.PhotoId")
                            .Results
                            .ToList();
        }

        public Video FindVideo(int postId)
        {
            _client.Connect();

            return _client.Cypher.Match("(po:post {PostId:" + postId + "})-[:HAS]->(v:video)")
                            .Return<Video>("v")
                            .OrderBy("v.VideoId")
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
                 * match (p:post{PostId:@PostId})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                    return u
                 */
            _client.Connect();
            var user = _client.Cypher.Match("(p:post{PostId:" + post.PostId + "})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)")
                .Return<User>("u")
                .Results.FirstOrDefault();
            return user;
        }

        public User FindUserByPostInRoom(int postId)
        {
            /*
                 * Query:
                 * Find:
                 *     - find User has post
                 * 
                 * match (p:post{PostId:@PostId})-[:PREV_POST*0..]-(p1:post)-[:LATEST_POST]-(u:user)
                    return u
                 */
            _client.Connect();
            var user = _client.Cypher.Match("(p:post{PostId:" + postId + "})<-[:CREATE]-(u:user)")
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
                .Where("p.Privacy='public'")
                .Return<Post>("p")
                .OrderByDescending("p.DateCreated")
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
                 * match (po:post {PostId:@PostId})-[:at]->(pl:place)
                    return pl
                 */

            _client.Connect();
            Place place;
            try
            {
                place = _client.Cypher.Match("(po:post {PostId:" + po.PostId + "})-[:AT]->(pl:place)")
                    .Return<Place>("pl")
                    .OrderBy("pl.PlaceId")
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
            return _client.Cypher.Match("(pl:place {Name: '" + place.Name + "'})")
                                .Where("abs(pl.Longitude - " + place.Longitude + ") < 0.000000001  and abs(pl.Latitude - " + place.Latitude + ") < 0.000000001")
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
                 * match (u1:user{UserId:@otherUserID})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post), (u2:user{UserId:@UserId})
                    where p1.privacy = 'public' or (p1.privacy = 'friend' and u1-[:FRIEND]-u2)
                    return p1
                 */
            _client.Connect();
            var listPost = _client.Cypher.Match("(u1:user {UserId:" + otherUser.UserId + "})-[:LATEST_POST]-(p:post)-[:PREV_POST*0..]-(p1:post), (u2:user {UserId:" + currentUser.UserId + "})")
                .Where("p1.Privacy = 'public' or (p1.Privacy = 'friend' and u1-[:FRIEND]-u2)")
                .ReturnDistinct<Post>("p1")
                //.OrderByDescending("p.DateCreated")
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
                 * optional match (u1:user{UserId:@UserId})-[:LATEST_POST]-(a:post)-[:PREV_POST*0..]-(p1:post)
                    optional match (u1)-[:FRIEND]-(u2:user)-[:LATEST_POST]-(b:post)-[:PREV_POST*0..]-(p2:post)
                    with collect(distinct p1) as list1, collect(distinct p2) as list2
                    match (p3:post)
                    where p3.privacy = 'public' or (p3 in list1) or (p3 in list2 and p3.privacy='friend')
                    return distinct p3
                 */
            _client.Connect();
            var listPost = _client.Cypher.OptionalMatch("(u1:user{UserId:" + user.UserId + "})-[:LATEST_POST]-(a:post)-[:PREV_POST*0..]-(p1:post)")
                .OptionalMatch("(u1)-[:FRIEND]-(u2:user)-[:LATEST_POST]-(b:post)-[:PREV_POST*0..]-(p2:post)")
                .With("collect(distinct p1) as list1, collect(distinct p2) as list2")
                .Match("(p3:post)")
                .Where("p3.Privacy = 'public' or (p3 in list1) or (p3 in list2 and p3.Privacy='friend')")
                .ReturnDistinct<Post>("p3")
                .OrderByDescending("p3.DateCreated")
                .Skip(skip)
                .Limit(limit)
                .Results.ToList();
            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public Post FindPost(int postId, User user)
        {
            /*
                 * Query:
                 * Find:
                 *     - post of current user not 'lock'
                 *     - post with privacy = 'public'
                 *     - post of friend with privacy = 'friend'
                 * 
                 * MATCH (u:user { UserId:@UserId }),(p:post { PostId: @PostId}), c = shortestPath((u)-[:LATEST_POST|:PREV_POST|:FRIEND*..]-(p))
                    RETURN COUNT(c)
                 */
            _client.Connect();
            var p = _client.Cypher.Match("(p:post {PostId:" + postId + "})")
                .ReturnDistinct<Post>("p")
                .Results.SingleOrDefault();
            if (p == null || !p.Privacy.Equals("public"))
            {
                int path = _client.Cypher.Match("(u:user {UserId:" + user.UserId + "}),(p:post { PostId: " + postId + "}), c = shortestPath((u)-[:LATEST_POST|:PREV_POST|:FRIEND*..]-(p))")
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
                .Where("p.Privacy='public'")
                .Return<Post>("p")
                .OrderByDescending("p.DateCreated")
                .Skip(skip)
                .Limit(limit)
                .Results.ToList();

            listPost.RemoveAll(item => item == null);
            return listPost;
        }

        public bool InsertComment(int postId, Comment comment, int userId)
        {
            // Auto increment Id
            comment.CommentId = GetGlobalIncrementId();

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
                 * match (u:user{UserId:@otherUserID})-[r:COMMENTED]->(p:post{PostId:@PostId})
                    delete r
                    create (u)-[r1:COMMENTED {DateCreated: @now, ActivtyId : @ActivtyId}]->(p)
                 */
                User otherUser = FindUser(comment);
                _client.Cypher.Match("(u:user{UserId:" + otherUser.UserId + "})-[r:COMMENTED]->(p:post{PostId:" + postId + "})")
                            .Delete("r").
                            ExecuteWithoutResults();

                _client.Cypher.Match("(u:user{UserId:" + otherUser.UserId + "}), (p:post{PostId:" + postId + "})")
                            .Create("(u)-[r1:COMMENTED {DateCreated: '" + DateTime.Now.ToString(FapConstants.DatetimeFormat) + "', ActivtyId : " + GetActivityIncrementId() + "}]->(p)")
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
                int oldComment = _client.Cypher.Match("(p:post{PostId:" + postId + "})-[:LATEST_COMMENT]->(c:comment)")
                                    .Return<int>("COUNT (c)")
                                    .Results.Single();

                if (oldComment == 0)
                {
                    // CREATE New LATEST_COMMENT
                    _client.Cypher.Match("(p:post{PostId:" + postId + "}), (c:comment{CommentId:" + comment.CommentId + "})")
                                    .Create("(p)-[:LATEST_COMMENT]->(c)")
                                    .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.Match("(p:post{PostId:" + postId + "})-[r:LATEST_COMMENT]->(c:comment), (c1:comment{CommentId:" + comment.CommentId + "})")
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
                * optional match (u:user {UserId:10000})-[:LATEST_POST]-(p1:post)-[:PREV_POST*0..]-(p:post)
                with p, u
                optional match (p)<-[m:COMMENTED|:LIKE|:DISLIKE]-(u1:user)
                WHERE u1.UserId <> u.UserId and m.ActivtyId < @ActivtyId
                m.DateCreated as DateCreated, p, u1, type(m) as activity, m.ActivtyId as lastActivityID
                return DateCreated, u1, p, activity, lastActivityID
                ORDER BY m.DateCreated
                limit @limit
            */

            string limitActivity = "";
            if (activityId != 0)
            {
                limitActivity = "and m.ActivtyId < " + activityId;
            }
            _client.Connect();
            var listNotification = _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "})-[:LATEST_POST]-(p1:post)-[:PREV_POST*0..]-(p:post)")
                .With("p, u")
                .OptionalMatch("(p)<-[m:COMMENTED|:LIKE|:DISLIKE]-(u1:user)")
                .Where("u1.UserId <> u.UserId " + limitActivity)
                .With("m.DateCreated as dateCreated, p, u1, type(m) as activity, m.ActivtyId as lastActivityId")
                .Return((dateCreated, u1, p, activity, lastActivityId) => new Notification
                {
                    DateCreated = dateCreated.As<String>(),
                    Activity = activity.As<String>(),
                    LastActivityId = lastActivityId.As<Int16>(),
                    User = u1.As<User>(),
                    Post = p.As<Post>()
                })
                //.Return<Notification>("distinct m.DateCreated as DateCreated, u1 as user, p as post")
                .OrderByDescending("dateCreated, lastActivityId")
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
            _client.Cypher.Match("(n:user { Email: '" + email + "' })")
                           .Set("n.Password = '" + newPassword + "' RETURN n")
                           .ExecuteWithoutResults();
        }

        public bool EditProfile(User user)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(n:user { UserId: " + user.UserId + "})")
                           .Set("n.FirstName = '" + user.FirstName + "'")
                           .Set("n.LastName = '" + user.LastName + "'")
                           .Set("n.Address = '" + user.Address + "'")
                           .Set("n.Gender = '" + user.Gender + "'")
                           .Set("n.PhoneNumber = '" + user.PhoneNumber + "'")
                           .Set("n.DateOfBirth = '" + user.DateOfBirth + "'")
                           .Set("n.Password = '" + user.Password + "'")
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
                _client.Cypher.Match("(c:comment { CommentId: " + comment.CommentId + " })")
                       .Set("c.DateCreated = '" + comment.DateCreated + "'")
                       .Set("c.Content = '" + comment.Content + "'")
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
                var prev = _client.Cypher.OptionalMatch("(c:comment {CommentId:" + commentId + "})<-[:PREV_COMMENT*1..1]-(c_prev:comment)")
                    .Return<Comment>("c_prev")
                    .Results.SingleOrDefault();

                var next = _client.Cypher.OptionalMatch("(c:comment {CommentId:" + commentId + "})-[:PREV_COMMENT*1..1]->(c_next:comment)")
                    .Return<Comment>("c_next")
                    .Results.SingleOrDefault();

                var post = _client.Cypher.OptionalMatch("(c:comment {CommentId:" + commentId + "})-[:PREV_COMMENT*0..]-(c1:comment)-[:LATEST_COMMENT]-(p:post)")
                    .Return<Post>("p")
                    .Results.SingleOrDefault();

                if (prev == null)
                {
                    if (next != null)
                    {
                        if (post != null)
                            _client.Cypher.Match("(c:comment {CommentId:" + next.CommentId + "}), (p:post {PostId: " + post.PostId + "})")
                                .Create("p-[:LATEST_COMMENT]->c")
                                .ExecuteWithoutResults();
                    }
                }
                else
                {
                    if (next != null)
                    {
                        _client.Cypher.Match("(c:comment {CommentId:" + prev.CommentId + "}), (c1:comment {CommentId: " + next.CommentId + "})")
                                .Create("c-[:PREV_COMMENT]->c1")
                                .ExecuteWithoutResults();
                    }
                }

                _client.Cypher.Match("(c:comment {CommentId:" + commentId + "})-[r]-()")
                                .Delete("c,r")
                                .ExecuteWithoutResults();

                if (post != null)
                {
                    var newLast = _client.Cypher.OptionalMatch("(p:post {PostId:" + post.PostId + "})-[:LATEST_COMMENT]-(c:comment)-[:PREV_COMMENT*0..]-(c1:comment)")
                        .Where("(:user {UserId:" + userId + "})-[:CREATE]->(c1)")
                        .Return<Comment>("c1")
                        .OrderByDescending("c1.DateCreated")
                        .Results.FirstOrDefault();

                    if (newLast == null)
                    {
                        _client.Cypher.OptionalMatch("(u:user {UserId: " + userId + "})-[r:COMMENTED]->(p:post {PostId:" + post.PostId + "})")
                                     .Delete("r")
                                     .ExecuteWithoutResults();
                    }
                    else
                    {
                        _client.Cypher.OptionalMatch("(u:user {UserId: " + userId + "})-[r:COMMENTED]->(p:post {PostId:" + post.PostId + "})")
                                     .Set("r.DateCreated = '" + newLast.DateCreated + "'")
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
                message = _client.Cypher.OptionalMatch("(c:conversation { ConversationId: " + conversationId + "})-[:LATEST_MESSAGE]-(m:message)")
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
            List<Message> listMessage;
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

                listMessage = _client.Cypher.OptionalMatch("(c:conversation { ConversationId: " + conversationId + "})-[:LATEST_MESSAGE]-(m:message)-[:PREV_MESSAGE*0.." + limit + "]-(m1:message)")
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
                 * MATCH(u:user)-[:CREATE]->(m:message{MessageId:@MessageId})
                    return u
                 */
            if (message == null)
            {
                return null;
            }
            _client.Connect();
            var user = _client.Cypher.OptionalMatch("(u:user)-[:CREATE]->(m:message{MessageId:" + message.MessageId + "})")
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

            return CreateMessage(content, userId, 0, conversation.ConversationId);
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
                    Content = content,
                    DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                    MessageId = GetGlobalIncrementId()
                };

                _client.Cypher.Create("(m:message {newMessage})")
                                    .WithParam("newMessage", message)
                                    .ExecuteWithoutResults();

                if (conversationId == -1)
                {
                    var conversation = new Conversation
                    {
                        DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat),
                        ConversationId = GetGlobalIncrementId()
                    };

                    _client.Cypher.Create("(c:conversation {newConversation})")
                                        .WithParam("newConversation", conversation)
                                        .ExecuteWithoutResults();

                    _client.Cypher.Match("(c:conversation {ConversationId: " + conversationId + "}), (u:user {UserId:" + userId + "}), (u1:user {UserId:" + otherId + "}), (m:message {MessageId:" + message.MessageId + "})")
                                        .Create("u-[:BELONG_TO]->c")
                                        .Create("u1-[:BELONG_TO]->c")
                                        .Create("c-[:LATEST_MESSAGE]->m")
                                        .Create("u-[:CREATE]->m")
                                        .ExecuteWithoutResults();
                }
                else
                {
                    _client.Cypher.OptionalMatch("(c:conversation {ConversationId: " + conversationId + "})-[r:LATEST_MESSAGE]->(m1:message), (u:user {UserId:" + userId + "}), (m:message {MessageId:" + message.MessageId + "})")
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

            var conversationIdList = _client.Cypher.Match("(u1:user{UserId: " + userId +
                                 "})-[:BELONG_TO]->(c:conversation)<-[:BELONG_TO]-(u2:user{UserId: " + otherId + "})")
                .ReturnDistinct<int>("c.ConversationId")
                .Results
                .ToList();

            return conversationIdList.Any() ? conversationIdList.First() : -1;
        }

        public bool SendRequestFriend(int userId, int otherUserId)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(u:user {UserId:" + userId + "}), (u1:user {UserId:" + otherUserId + "})")
                                    .Create("u-[:FRIEND_REQUEST {DateCreated: '" + DateTime.Now.ToString(FapConstants.DatetimeFormat) + "'}]->u1")
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
                _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:FRIEND_REQUEST]-(u1:user {UserId:" + otherUserId + "})")
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
                _client.Cypher.Match("(u:user {UserId:" + userId + "}), (u1:user {UserId:" + otherUserId + "})")
                                    .Create("u-[:FRIEND]->u1")
                                    .Create("u1-[:FRIEND]->u")
                                    .ExecuteWithoutResults();

                _client.Cypher.OptionalMatch("(u:user {UserId:" + userId + "})-[r:FRIEND_REQUEST]-(u1:user {UserId:" + otherUserId + "})")
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
                _client.Cypher.Match("(u:user {UserId:" + userId + "})-[r:FRIEND]-(u1:user {UserId:" + otherUserId + "})")
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
            var listUser = _client.Cypher.OptionalMatch("(u1:user {UserId:" + userId + "})<-[:FRIEND_REQUEST]-(u2:user)")
                .ReturnDistinct<User>("u2")
                .Results.ToList();
            listUser.RemoveAll(item => item == null);
            return listUser;
        }

        public bool DeletePost(int postId)
        {
            _client.Connect();
            User user = SearchUser(postId);

            DeleteRelatedPhotosAndRelationships(postId, user.UserId);
            DeletePlaceRelationship(postId);

            try
            {
                //TODO: Remove likes, dislike
                DeleteCommentsAndRelationship(postId);
                RebuildPostsChainFlow(user.UserId, postId);
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
            _client.Cypher.Match("(post{PostId: " + postId + "})-[lcr:LATEST_COMMENT]->(lc:comment)-[pcrs:PREV_COMMENT*]->(pc:comment), ()-[ccrl:CREATE]->lc,()-[ccrp:CREATE]->pc")
                          .ForEach("(pcr IN pcrs| DELETE pcr)")
                         .Delete("lcr, ccrl, ccrp, lc, pc")
                         .ExecuteWithoutResults();
        }

        private void DeletePostAndRelationships(int postId)
        {
            _client.Cypher.Match("(p:post {PostId:" + postId + "})-[r]-()")
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
                    _client.Cypher.Match("(u:user {UserId:" + userId + "}), (p:post {PostId: " + next.PostId + "})")
                        .Create("(u)-[:LATEST_POST]->(p)")
                        .ExecuteWithoutResults();
                }
            }
            else
            {
                if (next != null)
                {
                    _client.Cypher.Match("(pp:post {PostId:" + prev.PostId + "}), (pn:post {PostId: " + next.PostId + "})")
                        .Create("pp-[:PREV_POST]->pn")
                        .ExecuteWithoutResults();
                }
            }
        }

        private Post FindNextPost(int postId)
        {
            return _client.Cypher.OptionalMatch("(p:post {PostId:" + postId + "})-[:PREV_POST*1..1]->(p_next:post)")
                .Return<Post>("p_next")
                .Results.SingleOrDefault();
        }

        private Post FindPrevPost(int postId)
        {
            return _client.Cypher.OptionalMatch("(p:post {PostId:" + postId + "})<-[:PREV_POST*1..1]-(p_prev:post)")
                .Return<Post>("p_prev")
                .Results.SingleOrDefault();
        }

        private void DeletePlaceRelationship(int postId)
        {
            _client.Cypher.Match("(post{PostId: " + postId + "})-[r1:AT]->(place)<-[r2:VISITED]-(user)")
                         .Delete("r1, r2")
                         .ExecuteWithoutResults();
        }

        private void DeleteRelatedPhotosAndRelationships(int postId, int userId)
        {
            _client.Cypher.OptionalMatch("(p:Post {PostId: " + postId + "})-[r1:HAS]-(pt:Photo)<-[r2:HAS]-(u:User {UserId: " + userId + "})")
                .Delete("r1, r2, pt")
                .ExecuteWithoutResults();
        }
        #endregion

        public bool EditPost(int postId, string newContent)
        {
            _client.Connect();
            try
            {
                _client.Cypher.Match("(p:post { PostId: " + postId + " })")
                       .Set("p.DateCreated = '" + DateTime.Now.ToString(FapConstants.DatetimeFormat) + "'")
                       .Set("p.Content = '" + newContent + "'")
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
                       .Where("upper(u.FirstName + ' ' + u.LastName) =~ '.*" + keyword + ".*'")
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
                   .Where("upper(p.Name) =~ '.*" + keyword + ".*'")
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

        public List<Room> SearchRoomByUserId(int userId)
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
                   .OptionalMatch("(u:user {UserId: " + userId + "})-[m:JOIN{type:" + FapConstants.JoinAdmin + "}]->(r:room)")
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
             * match(pl:place {PlaceId:@PlaceId})<-[:AT]-(p:post)
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
                   .Match("(pl:place {PlaceId:" + placeId + "})<-[:AT]-(p:post)")
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
             * optional match (pl:place {PlaceId:@PlaceId})<-[:AT]-(p:post)
                return COUNT(p)
             */
            int numberOfPost;
            _client.Connect();
            try
            {
                numberOfPost = _client.Cypher
                   .OptionalMatch("(pl:place {PlaceId:" + placeId + "})<-[:AT]-(p:post)")
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


        public bool InsertPlan(Plan newPlan, int roomId, int userId, List<int> assigneesId = null)
        {
            newPlan.PlanId = GetActivityIncrementId();
            _client.Connect();

            _client.Cypher
                   .Create("(plan:plan {newPlan})")
                   .WithParam("newPlan", newPlan)
                   .ExecuteWithoutResults();

            _client.Cypher.Match("(r:room {RoomId:" + roomId + "}), (p:plan {PlanId: " + newPlan.PlanId + "})")
                         .Create("(r)-[:HAS]->(p)")
                         .ExecuteWithoutResults();

            _client.Cypher.Match("(u:user {UserId: " + userId + "}), (p:plan {PlanId: " + newPlan.PlanId + "})")
                         .Create("(u)-[r:CREATE]->(p)")
                         .ExecuteWithoutResults();

            if (assigneesId != null)
            {
                foreach (int assigneeId in assigneesId)
                {
                    _client.Cypher.Match("(u:user {UserId: " + assigneeId + "}), (p:plan {PlanId: " + newPlan.PlanId + "})")
                                 .Create("(u)-[:IN_CHARGE]->(p)")
                                 .ExecuteWithoutResults();
                }
            }

            return true;
        }

        public List<Plan> LoadAllPlansInDateRange(DateTime fromDate, DateTime toDate, int planType, int roomid)
        {
            //DateTime fromDate = DateHelpers.Instance.ConvertFromUnixTimestamp(start) ?? DateTime.MinValue;

            List<Plan> listPlan;
            _client.Connect();
            try
            {
                listPlan = _client.Cypher
                       .Match("(:room{RoomId: " + roomid + "})-[:HAS]->(p:plan {PlanType: " + planType + "})")
                       .ReturnDistinct<Plan>("p")
                       .OrderBy("p.DatePlanStart DESC")
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
                    .Where("p2.Privacy <> 'lock'")
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
                admin = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})<-[j:JOIN {type:" + FapConstants.JoinAdmin + "}]-(u:user)")
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
                listUser = _client.Cypher.OptionalMatch("(r:room {RoomId:" + roomId + "})<-[j:JOIN {type:" + FapConstants.JoinRequest + "}]-(u:user)")
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
                    //.Where("p1.MessageId < " + MessageId)
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

        public List<Post> FindPostInPlace(int placeId, int postId = 0, int limit = 5)
        {
            /*
                 * Query:
                 * Find:
                 *     optional match (pl:place {PlaceId:@PlaceId})<-[:AT]-(p:post)
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
                listPost = _client.Cypher.OptionalMatch("(pl:place {PlaceId:" + placeId + "})<-[:AT]-(p:post)")
                                        .OptionalMatch("(p)<-[:LIKE]-(ul:user)")
                                        .OptionalMatch("(p)<-[:DISLIKE]-(udl:user)")
                                        .OptionalMatch("(p)-[:LATEST_COMMENT]->(c:comment)-[PREV_COMMENT*0..]->(c1:comment)")
                                        .With("p, (COUNT(DISTINCT ul)) - (COUNT(DISTINCT udl)) + (COUNT(DISTINCT c1)) as mark")
                                        .OrderByDescending("mark")
                                        .Return<Post>("p")
                    //.Where("p2.id < " + PostId)
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

        public Place FindPlaceById(int placeId)
        {

            /*
                 * Query:
                 * Find:
                 *     - Find Place by id
                 */

            _client.Connect();
            Place place;
            try
            {
                place = _client.Cypher.OptionalMatch("(pl:place {PlaceId:" + placeId + "})")
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

        public Place FindPlaceByCoordinate(double longitude, double latitude)
        {
            /*
                 * Query:
                 * Find:
                 */

            _client.Connect();
            Place place;
            try
            {
                place = _client.Cypher.OptionalMatch("(pl:place {Longitude:" + longitude + ", Latitude: " + latitude + "})")
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
                   .Create("(r:room {newPlan})")
                   .WithParam("newPlan", newRoom)
                   .ExecuteWithoutResults();

            var newConversation = new Conversation
            {
                ConversationId = GetGlobalIncrementId(),
                DateCreated = DateTime.Now.ToString(FapConstants.DatetimeFormat, CultureInfo.InvariantCulture)
            };

            _client.Cypher.Match("(u:user {UserId:" + currentUserId + "}), (r:room {RoomId: " + newRoom.RoomId + "})")
                     .Create("(u)-[:JOIN {type: " + FapConstants.JoinAdmin + "}]->(r)")
                     .ExecuteWithoutResults();

            _client.Cypher
                   .Create("(c:conversation {newConversation})")
                   .WithParam("newConversation", newConversation)
                   .ExecuteWithoutResults();

            _client.Cypher.Match("(r:room {RoomId: " + newRoom.RoomId + "}), (c:conversation {ConversationId: " + newConversation.ConversationId + "})")
                     .Create("(r)-[:HAS]->(c)")
                     .ExecuteWithoutResults();
        }

        public Room GetRoomInformation(int roomId)
        {
            _client.Connect();
            return _client.Cypher.Match("(r:room{RoomId: " + roomId + "})")
                .Return<Room>("r")
                .Results
                .FirstOrDefault();
        }

        public IEnumerable<User> GetPersonInCharge(int planId)
        {
            _client.Connect();
            return _client.Cypher.Match("(u:user)-[:IN_CHARGE]->(:plan{PlanId: " + planId + "})")
                .Return<User>("u")
                .Results;
        }
        public User GetPersonCreatesPlan(int planId)
        {
            _client.Connect();
            return _client.Cypher.Match("(u:user)-[:CREATE]->(:plan{PlanId: " + planId + "})")
                .Return<User>("u")
                .Results
                .FirstOrDefault();
        }
    }
}