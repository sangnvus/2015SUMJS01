var homeModule = (function () {
    var isCallAjax = false;
    var pageNumberIndex = 1;
    var pageNumberWish = 1;
    var pageNumberHot = 1;

    var setBlocksit = function () {
        commonModule.setBlocksit($(".blog-landing"), 212);
    };

    var fadeImage = function () {
        $(".dark-wrapper").hover(function () {
            $(this).find("img").stop().fadeTo(500, 0.5);

            var postActions = $(this).find(".post-actions");

            postActions.css("visibility", "visible");
            postActions.css("opacity", "1");
        }, function () {
            $(this).find("img").stop().fadeTo(500, 1);

            var postActions = $(this).find(".post-actions");

            postActions.css("visibility", "hidden");
            postActions.css("opacity", "0");
        });
    };

    var likePost = function () {
        $(".btn-like").click(function (evt) {
            $(this).toggleClass("btn-primary").toggleClass("btn-warning");
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
            evt.preventDefault();
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

            var buttonLike = $(post).parentsUntil(".white-panel")
                                .parent()
                                .find(".btn-like");
            $(buttonLike).toggleClass("btn-primary").toggleClass("btn-warning");
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

            var buttonDislike = $(post).parentsUntil(".white-panel")
                .parent()
                .find(".btn-dislike");
            $(buttonDislike).toggleClass("btn-primary").toggleClass("btn-warning");
        }
    };
    var dislikePost = function () {
        $(".btn-dislike").click(function (evt) {
            $(this).toggleClass("btn-primary").toggleClass("btn-warning");
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
            evt.preventDefault();
        });
    };

    var plusPost = function () {
        // Add to wish list
        $(".btn-plus").click(function (evt) {
            var controller = "";
            var data = {
                postID: parseInt($(this).attr("role"))
            };
            if ($(this).hasClass("btn-primary")) {
                controller = "/User/AddToWishlist";
            }
            else if ($(this).hasClass("btn-warning")) {
                controller = "/User/RemoveFromWishlist";
            }
            $(this).toggleClass("btn-primary").toggleClass("btn-warning");

            if (controller != "") {
                commonModule.callAjax(controller, data, null);
            }
            evt.preventDefault();
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
    var loadMoreData = function (typePost) {
        if (isCallAjax) {
            return;
        }
        isCallAjax = true;
        var controller = "/Home/LoadMore/";
        var data = {
            pageNumber: 1
        };
        if (typePost == "wish") {
            controller = "/Home/LoadMoreWish/";
            data.pageNumber = pageNumberWish++;
        } else if (typePost == "hot") {
            controller = "/Home/LoadMoreHot/";
            data.pageNumber = pageNumberHot++;
        } else {
            //  (typePost == "index")
            controller = "/Home/LoadMore/";
            data.pageNumber = pageNumberIndex++;
        }

        $("div#loading").show();
        $.ajax({
            type: 'POST',
            url: controller,
            data: data,
            success: function (data, textstatus) {
                $(".blog-landing").append(data);
                homeModule.setBlocksit();
                homeModule.fadeImage();
                homeModule.likePost();
                homeModule.dislikePost();
                homeModule.plusPost();
                $("div#loading").hide();
                isCallAjax = false;
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("div#loading").hide();
                isCallAjax = false;
            }
        });
    };

    return {
        setBlocksit: setBlocksit,
        fadeImage: fadeImage,
        likePost: likePost,
        dislikePost: dislikePost,
        plusPost: plusPost,
        loadMoreData: loadMoreData
    }
})();

$(window).scroll(function () {
    if ($(window).scrollTop() === $(document).height() - $(window).height()) {
        if (isLoadMore || isLoadMore === "true") {
            homeModule.loadMoreData();
        }
    }
});

$(window).load(function () {
    homeModule.setBlocksit();
    homeModule.fadeImage();
    homeModule.likePost();
    homeModule.dislikePost();
    homeModule.plusPost();
    $("div#loading").hide();
});

//window resize
$(window).resize(function () {
    homeModule.setBlocksit();
});