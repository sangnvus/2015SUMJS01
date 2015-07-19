$(document).ready(function () {
    commentHubProxy = $.connection.commentHub;

    commentHubProxy.client.addNewComment = function (content) {
        $(".comment-list").append(content);
    }

    $(window).load(function () {
        postDetailModule.likePost();
        postDetailModule.dislikePost();
    });

    $.connection.hub.start().done(function () {
        $("#idTextareaComment").keydown(function (e) {
            postDetailModule.handleEnter(e);
        });
    });
    $("#idTextareaComment").elastic();
});

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
    
    var commentAjax = function (post) {
        var controller = "/User/Comment";
        var postID = parseInt($(post).attr("role"));
        var content = $(post).val();
        var data = {
            postId: postID,
            content: content
        }

        $.ajax({
            type: 'POST',
            url: controller,
            data: data,
            success: function (data, textstatus) {
                $(".comment-list").append(data);
                $("#idTextareaComment").text("").val("");

                commentHubProxy.server.sendComment(data);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }

    var handleEnter = function (evt) {
        if (evt.keyCode == 13 && evt.shiftKey) {
            if (evt.type == "keydown") {
                // create new line
                $(this).val($(this).val() + "\n");
            }
            evt.preventDefault();
        }
        else if (evt.keyCode == 13) {
            commentAjax($("#idTextareaComment"));
            evt.preventDefault();
        }
    };

    return {
        likePost: likePost,
        dislikePost: dislikePost,
        handleEnter: handleEnter
    }
})();