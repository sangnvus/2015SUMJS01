var postDetailModule = (function () {
    var likePost = function () {
        $(".btn-like").click(function () {
            var likeIcon = $(".fa-thumbs-o-up");

            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");

            var likeCountElement = $(".like-count");

            if (likeIcon.hasClass("interacted")) {
                likeCountElement.text(parseInt(likeCountElement.text()) + 1);
            } else {
                likeCountElement.text(parseInt(likeCountElement.text()) - 1);
            }

            unDislikePost();
            likeAjax(this);
        });
    };

    var unLikePost = function () {
        var likeIcon = $(".fa-thumbs-o-up");

        var likeCountElement = $(".like-count");

        if (likeIcon.hasClass("interacted")) {
            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");
            likeCountElement.text(parseInt(likeCountElement.text()) - 1);
        }
    };

    var unDislikePost = function () {
        var dislikeIcon = $(".fa-thumbs-o-down");

        var dislikeCountElement = $(".dislike-count");

        if (dislikeIcon.hasClass("interacted")) {
            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");
            dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
        }
    };

    var dislikePost = function () {
        $(".btn-dislike").click(function () {
            var dislikeIcon = $(".fa-thumbs-o-down");

            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");

            var dislikeCountElement = $(".dislike-count");

            if (dislikeIcon.hasClass("interacted")) {
                dislikeCountElement.text(parseInt(dislikeCountElement.text()) + 1);
            } else {
                dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
            }

            unLikePost();
            dislikeAjax(this);
        });
    };

    var likeAjax = function (post) {
        var controller = "/User/Like";
        var postID = parseInt($(post).attr("role"));
        var data = {
            postId: postID
        }

        commonModule.callAjax(controller, data, null);
    };

    var dislikeAjax = function (post) {
        var controller = "/User/Dislike";
        var postID = parseInt($(post).attr("role"));
        var data = {
            postId: postID
        }

        commonModule.callAjax(controller, data, null);
    };

    return {
        likePost: likePost,
        dislikePost: dislikePost
    }
})();



$(document).ready(function () {
    $(window).load(function () {
        postDetailModule.likePost();
        postDetailModule.dislikePost();
    });
});