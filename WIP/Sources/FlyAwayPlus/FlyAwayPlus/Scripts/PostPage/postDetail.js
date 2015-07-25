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
    
    var editCommentAjax = function (comment) {
        var controller = "/User/EditComment";
        var commentID = parseInt($(comment).attr("role"));
        var content = $(comment).val();
        var data = {
            commentID: commentID,
            content: content
        }

        $.ajax({
            type: 'POST',
            url: controller,
            data: data,
            success: function (data, textstatus) {
                // send notification
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }

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

    var deleteComment = function (commentID) {
        var controller = "/User/DeleteComment";
        var data = {
            commentID: commentID
        }

        $.ajax({
            type: 'POST',
            url: controller,
            data: data,
            success: function (data, textstatus) {
                // send notification
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }

    var handleEnter = function (evt, selector) {
        if (evt.keyCode == 13 && evt.shiftKey) {
            if (evt.type == "keydown") {
                // create new line
                $(this).val($(this).val() + "\n");
            }
            evt.preventDefault();
        }
        else if (evt.keyCode == 13) {
            if (selector) {
                if ($(selector).val() != "") {
                    editCommentAjax($(selector));
                    var parent = $(selector).closest(".comment-content");
                    $(selector).hide();
                    $(parent).find("p").text("").append($(selector).val().split("\n").join("<br />"));
                    $(parent).find("p").show();
                }
            }
            else {
                if ($("#idTextareaComment").val() != "") {
                    commentAjax($("#idTextareaComment"));
                }
            }
            evt.preventDefault();
        }
    };

    var editCommentEvent = function (selector) {
        var checkOutFocus = false;
        var checkMouse = false;
        $(".comment-content").find("textarea").focusout(function (evt) {
            var content = $(this).closest(".comment-content").find("p");
            if (!checkOutFocus && checkMouse) {
                $(content).show();
                $(this).hide();
            }
            evt.preventDefault();
        });

        $(document).mousedown(function (e) {
            var container = $(".comment-content").find("textarea");
            if (container.has(e.target).length === 0) {
                checkMouse = true;
            }
            else {
                checkMouse = false;
            }
        });
        $(".commenter-edit").click(function (evt) {
            checkOutFocus = true;
            var content = $(this).closest(".comment-detail").find(".comment-content");
            var textarea = $(content).find("textarea");

            $(textarea).val($(textarea).val().split("<br />").join("\n"));
            $(content).find("p").hide();
            $(textarea).show();

            $(textarea).keydown(function (e) {
                checkMouse = false;
                postDetailModule.handleEnter(e, textarea);
            });

            $(textarea).elastic();
            checkOutFocus = false;
            evt.preventDefault();
        });

        $(".commenter-edit").click(function (evt) {
            checkOutFocus = true;
            var content = $(this).closest(".comment-detail").find(".comment-content");
            var textarea = $(content).find("textarea");

            $(textarea).val($(textarea).val().split("<br />").join("\n"));
            $(content).find("p").hide();
            $(textarea).show();

            $(textarea).keydown(function (e) {
                checkMouse = false;
                postDetailModule.handleEnter(e, textarea);
            });

            $(textarea).elastic();
            checkOutFocus = false;
            evt.preventDefault();
        });
    };

    deleteCommentEvent = function () {
        $(".commenter-delete").click(function (evt) {
            var commentID = $(this).attr("role");
            $(this).closest(".comment-detail").remove();
            postDetailModule.deleteComment(commentID);
            evt.preventDefault();
        });
    };

    return {
        likePost: likePost,
        dislikePost: dislikePost,
        handleEnter: handleEnter,
        editCommentAjax: editCommentAjax,
        deleteComment: deleteComment,
        editCommentEvent: editCommentEvent,
        deleteCommentEvent: deleteCommentEvent
    }
})();

$(document).ready(function () {
    commentHubProxy = $.connection.commentHub;

    commentHubProxy.client.addNewComment = function (content) {
        $(".comment-list").append(content);
        postDetailModule.deleteCommentEvent();
        postDetailModule.editCommentEvent();
    }

    $(window).load(function () {
        postDetailModule.likePost();
        postDetailModule.dislikePost();
        postDetailModule.deleteCommentEvent();
        postDetailModule.editCommentEvent();
    });

    $.connection.hub.start().done(function () {
        $("#idTextareaComment").keydown(function (e) {
            postDetailModule.handleEnter(e);
        });
    });
    $("#idTextareaComment").elastic();
});