var postDetailModule = (function () {
    var likePost = function () {
        $(".btn-like").click(function () {
            var likeIcon = $(this).parent(".interact-area").find(".fa-thumbs-o-up");

            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");

            var likeCountElement = $(this).parent(".interact-area").find(".like-count");

            if (likeIcon.hasClass("interacted")) {
                likeCountElement.text(parseInt(likeCountElement.text()) + 1);
            } else {
                likeCountElement.text(parseInt(likeCountElement.text()) - 1);
            }

            unDislikePost(this);
        });
    };

    var unLikePost = function (post) {
        var likeIcon = $(post).parent(".interact-area").find(".fa-thumbs-o-up");

        var likeCountElement = $(post).parent(".interact-area").find(".like-count");

        if (likeIcon.hasClass("interacted")) {
            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");
            likeCountElement.text(parseInt(likeCountElement.text()) - 1);
        }
    };

    var unDislikePost = function (post) {
        var dislikeIcon = $(post).parent(".interact-area").find(".fa-thumbs-o-down");

        var dislikeCountElement = $(post).parent(".interact-area").find(".dislike-count");

        if (dislikeIcon.hasClass("interacted")) {
            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");
            dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
        }
    };

    var dislikePost = function () {
        $(".btn-dislike").click(function () {
            var dislikeIcon = $(this).parent(".interact-area").find(".fa-thumbs-o-down");

            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");

            var dislikeCountElement = $(this).parent(".interact-area").find(".dislike-count");

            if (dislikeIcon.hasClass("interacted")) {
                dislikeCountElement.text(parseInt(dislikeCountElement.text()) + 1);
            } else {
                dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
            }

            unLikePost(this);
        });
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