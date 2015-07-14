var homeModule = (function(){
    var setBlocksit = function () {
        var conWidth = $("#blog-landing").width();
        //my container width
        var gridwidth = 212;
        //alert(conWidth);
        var col = conWidth / gridwidth;
        //alert(col);
        $("#blog-landing").BlocksIt({
            numOfCol: Math.floor(col),
            offsetX: 8,
            offsetY: 8
        });
    };

    var fadeImage = function () {
        $(".dark-wrapper").hover(function () {
            $(this).find("img").fadeTo(500, 0.5);

            var postActions = $(this).find(".post-actions");

            postActions.css("visibility", "visible");
            postActions.css("opacity", "1");
            postActions.animate({ top: '20px' }, "slow");
        }, function () {
            $(this).find("img").fadeTo(500, 1);

            var postActions = $(this).find(".post-actions");

            postActions.animate({ top: '0px' }, "slow");
            postActions.css("visibility", "hidden");
            postActions.css("opacity", "0");
        });
    };

    var likePost = function () {
        $(".btn-like").click(function () {
            var likeIcon = $(this).parentsUntil(".white-panel")
                                  .parent()
                                  .find(".fa-thumbs-o-up");

            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");

            var likeCountElement = $(this).parentsUntil(".white-panel").parent()
                .find(".like-count");

            if (likeIcon.hasClass("interacted")) {
                likeCountElement.text(parseInt(likeCountElement.text()) + 1);
            } else {
                likeCountElement.text(parseInt(likeCountElement.text()) - 1);
            }

            unDislikePost(this);
            likeAjax(this);
        });
    };

    var unLikePost = function (post) {
        var likeIcon = $(post).parentsUntil(".white-panel")
                                .parent()
                                .find(".fa-thumbs-o-up");

        var likeCountElement = $(this).parentsUntil(".white-panel").parent()
            .find(".like-count");

        if (likeIcon.hasClass("interacted")) {
            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");
            likeCountElement.text(parseInt(likeCountElement.text()) - 1);
        }
    };

    var unDislikePost = function (post) {
        var dislikeIcon = $(post).parentsUntil(".white-panel")
                                .parent()
                                .find(".fa-thumbs-o-down");

        var dislikeCountElement = $(this).parentsUntil(".white-panel").parent()
            .find(".dislike-count");

        if (dislikeIcon.hasClass("interacted")) {
            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");
            dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
        }
    };

    var dislikePost = function () {
        $(".btn-dislike").click(function () {
            var dislikeIcon = $(this).parentsUntil(".white-panel")
                                  .parent()
                                  .find(".fa-thumbs-o-down");

            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");

            var dislikeCountElement = $(this).parentsUntil(".white-panel").parent()
                .find(".dislike-count");

            if (dislikeIcon.hasClass("interacted")) {
                dislikeCountElement.text(parseInt(dislikeCountElement.text()) + 1);
            } else {
                dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
            }

            unLikePost(this);
            dislikeAjax(this);
        });
    };

    var plusPost = function () {
        // Add to wish list
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
        setBlocksit: setBlocksit,
        fadeImage: fadeImage,
        likePost: likePost,
        dislikePost: dislikePost,
        plusPost: plusPost
    }
})();




$(window).load(function () {
    homeModule.setBlocksit();
    homeModule.fadeImage();
    homeModule.likePost();
    homeModule.dislikePost();
    homeModule.plusPost();
});
//window resize
$(window).resize(function () {
    homeModule.setBlocksit();
});