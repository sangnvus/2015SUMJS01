var postDetailModule = (function () {
    var isTagging = false;
    var _listFriend = [];
    var mapping = {};
    
    var likePost = function () {
        $(".btn-like").click(function (evt) {
            if (evt.handled !== true) { // This will prevent event triggering more then once
                evt.handled = true;
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
            }
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
        $(".btn-dislike").click(function (evt) {
            if (evt.handled !== true) { // This will prevent event triggering more then once
                evt.handled = true;
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
            }
        });
    };

    var likeAjax = function (post) {
        var controller = "/User/Like";
        var postId = parseInt($(post).attr("role"));
        var data = {
            postId: postId
        }

        commonModule.callAjax(controller, data, null);
        $.connection.hub.start().done(function () {
            sendNewNotification("LIKE");
        });
    };

    var dislikeAjax = function (post) {
        var controller = "/User/Dislike";
        var postId = parseInt($(post).attr("role"));
        var data = {
            postId: postId
        }

        commonModule.callAjax(controller, data, null);
        $.connection.hub.start().done(function () {
            sendNewNotification("DISLIKE");
        });
    };

    var editCommentAjax = function (comment) {
        var controller = "/User/EditComment";
        var commentId = parseInt($(comment).attr("role"));
        var content = $(comment).val();
        var data = {
            CommentId: commentId,
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
        var postId = parseInt($(post).attr("role"));
        var content = $(post).val();
        var data = {
            postId: postId,
            content: content
        }

        $.ajax({
            type: 'POST',
            url: controller,
            data: data,
            success: function (data, textstatus) {
                var value = data.split('&lt;br /&gt;').join('\\n');
                var pattern = /@([ \S]*?):/g;
                var tagFriend = value.match(pattern);
                var username = "";
                var replaceUser = "";

                if (tagFriend != null) {
                    for (var index = 0; index < tagFriend.length; index++) {
                        username = tagFriend[index].substring(1, tagFriend[index].length - 1);
                        replaceUser = "<a href='/User/Index/" + findUserID(username) + "'>" + username + "</a>";
                        value = value.split(tagFriend[index]).join(replaceUser);
                    }
                }

                $(".comment-list").append(value);
                $("#idTextareaComment").text("").val("");
                postDetailModule.deleteCommentEvent();
                postDetailModule.editCommentEvent();
                commentHubProxy.server.sendComment(value);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }

    var deleteComment = function (commentId) {
        var controller = "/User/DeleteComment";
        var data = {
            CommentId: commentId
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
        else if (evt.keyCode == 50 && evt.shiftKey) {
            isTagging = true;
            // input @ character
            createTagFriendUI("", selector);
        }
        else if (evt.keyCode == 13) {
            if (selector) {
                if ($(selector).val().trim().length > 0) {
                    editCommentAjax($(selector));
                    var parent = $(selector).closest(".comment-content");
                    $(selector).hide();

                    var value = $(selector).val().split("<br />").join("\n");
                    var pattern = /@([ \S]*?):/g;
                    var tagFriend = value.match(pattern);
                    var username = "";
                    var replaceUser = "";

                    if (tagFriend != null) {
                        for (var index = 0; index < tagFriend.length; index++) {
                            username = tagFriend[index].substring(1, tagFriend[index].length - 1);
                            replaceUser = "<a href='/User/Index/" + findUserID(username) + "'>" + username + "</a>";
                            value = value.split(tagFriend[index]).join(replaceUser);
                        }
                    }
                    $(parent).find("p").text("").append(value);
                    $(parent).find("p").show();
                }
                else {
                    swal({ title: "Warning!", text: "comment's content must not empty!", type: "warning", showConfirmButton: true });
                    //alert("comment's content must not empty!");
                }
            }
            else {
                if ($("#idTextareaComment").val() != "") {
                    commentAjax($("#idTextareaComment"));
                    try {
                        var commentCountArea = $("#idTextareaComment").closest(".comment-area").find(".comment-stats").find("span");
                        var list = $(commentCountArea).text().split(" ");
                        list[0] = parseInt(list[0]) + 1;
                        $(commentCountArea).text(list.join(" "));
                    } catch (e) {
                        console.log("Post Detail: Add comment " + e.message);
                    }
                    $.connection.hub.start().done(function () {
                        sendNewNotification("COMMENTED");
                    });
                }
            }
            evt.preventDefault();
        }
        else if (evt.keyCode == 8) {
            var obj = selector ? selector : $("#idTextareaComment");
            if (isTagging == true) {
                if ($(obj).val().length > 0 && $(obj).val().slice(-1) == '@') {
                    isTagging = false;
                    $("#id-tag-friend-tab-ui").hide();
                }
                else {
                    var obj = selector ? selector : $("#idTextareaComment");
                    var keyword = $(obj).val().substring($(obj).val().lastIndexOf("@") + 1, $(obj).val().length - 1);
                    createTagFriendUI(keyword, selector);
                }
            }
        } else {
            if (isTagging == true) {
                var obj = selector ? selector : $("#idTextareaComment");
                var keyword = $(obj).val().substring($(obj).val().lastIndexOf("@") + 1, $(obj).val().length) + String.fromCharCode((96 <= evt.keyCode && evt.keyCode <= 105) ? evt.keyCode - 48 : evt.keyCode);
                createTagFriendUI(keyword, selector);
            }
        }
        /*
        var obj = selector ? selector : $("#idTextareaComment");
        if ($(obj).val().indexOf('@') < 0) {
            createTagFriendUI("", selector);
            _isTagging = false;
        }*/
    };

    var editCommentEvent = function (selector) {
        var checkOutFocus = false;

        var obj = $(".commenter-edit");
        if (typeof (selector) != "undefined") {
            obj = $(selector);
        }
        $(obj).click(function (evt) {
            if (evt.handled !== true) { // This will prevent event triggering more then once
                checkOutFocus = true;
                var content = $(this).closest(".comment-detail").find(".comment-content");
                var textarea = $(content).find("textarea");

                $(textarea).val($(textarea).val().split("<br />").join("\n"));
                $(content).find("p").hide();
                $(textarea).show();

                $(textarea).keydown(function (e) {
                    //checkMouse = false;
                    postDetailModule.handleEnter(e, textarea);
                });

                $(textarea).elastic();
                checkOutFocus = false;

                evt.handled = true;
                evt.preventDefault();
            }
        });
    };

    deleteCommentEvent = function (selector) {
        var obj = $(".commenter-delete");
        if (typeof (selector) != "undefined") {
            obj = $(selector);
        }
        $(obj).click(function (evt) {
            var cmtDeleteSouce = $(this);

            swal({
                title: "Are you sure?",
                text: "You will not be able to recover this comment!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes, delete it!",
                closeOnConfirm: false
            }, function () {
                if (evt.handled !== true) { // This will prevent event triggering more then once
                    evt.handled = true;
                    var commentId = cmtDeleteSouce.attr("role");
                    var commentDetailArea = cmtDeleteSouce.closest(".comment-detail");
                    var commentCountArea = $(commentDetailArea).closest(".comment-area").find(".comment-stats").find("span");
                    commentDetailArea.remove();

                    postDetailModule.deleteComment(commentId);
                    try {
                        var list = $(commentCountArea).text().split(" ");
                        list[0] = parseInt(list[0]) - 1;
                        $(commentCountArea).text(list.join(" "));
                    } catch (e) {
                        console.log("Post Detail: Delete comment " + e.message);
                    }
                    evt.preventDefault();
                }

                swal({ title: "Deleted!", text: "Your comment has been deleted.", type: "success", timer: 1500, showConfirmButton: false });
            });
        });
    };

    isTagging = function () {
        return isTagging;
    };

    setTagging = function (value) {
        isTagging = value;
    };

    setListFriend = function (val) {
        _listFriend = val;
    };

    createTagFriendList = function (keyword) {
        var list = [];
        if (keyword == "") {
            return _listFriend;
        }
        for (var index = 0; index < _listFriend.length; index++) {
            var username = _listFriend[index]["FirstName"].toUpperCase() + " " + _listFriend[index]["LastName"].toUpperCase();
            if (username.indexOf(keyword.toUpperCase()) == 0) {
                list.push(_listFriend[index]);
            }
        }
        return list;
    };

    createTagFriendUI = function (keyword, selector) {
        var listFriend = createTagFriendList(keyword);
        var tmp = "";
        $("#id-tag-friend-list").html("");

        /*
            <li data-index="0" class="tab_complete_ui_item">
                <img class="lazy member_image thumb_24 member_preview_image" src="avatar.jpg">
                <span class="username">thanhdancer</span>
            </li>
        */
        for (var index = 0; index < Math.min(listFriend.length, 10) ; index++) {
            tmp = "<li data-index='" + index + "' class='tab_complete_ui_item' role='" + listFriend[index]["UserId"] + "'>";
            tmp += "<img class='lazy member_image thumb_24 member_preview_image' src='" + listFriend[index]["Avatar"] + "'>";
            tmp += "<span class='username'>" + listFriend[index]["FirstName"] + " " + listFriend[index]["LastName"] + "</span>";
            tmp += "</li>";
            $("#id-tag-friend-list").append(tmp);
        }

        var obj = selector ? selector : $("#idTextareaComment");
        $("#id-tag-friend-tab-ui").show();
        var offset = $(obj).offset();
        var top = offset.top;
        var left = offset.left;
        top = top - 1 - $("#id-tag-friend-tab-ui").height();
        $("#id-tag-friend-tab-ui").offset({ top: top, left: left }).width($(obj).width());

        $(".tab_complete_ui_item").click(function (evt) {
            if (evt.handled !== true) { // This will prevent event triggering more then once
                evt.handled = true;
                var username = $(this).find(".username").text();
                var userId = parseInt($(this).attr("role"));
                mapping[username] = userId;
                var currentText = $(obj).val().slice(0, $(obj).val().lastIndexOf('@') + 1);
                $(obj).val(currentText + username + ": ");
                $(obj).focus();
                $("#id-tag-friend-tab-ui").hide();
                isTagging = false;

                evt.preventDefault();
            }
        });
    };

    var findUserID = function (keyword) {
        return mapping[keyword];
        /*
        for (var index = 0; index < _listFriend.length; index++) {
            var username = _listFriend[index]["FirstName"].toUpperCase() + " " + _listFriend[index]["LastName"].toUpperCase();
            if (username.toUpperCase() == keyword.toUpperCase()) {
                return _listFriend[index]["UserId"];
            }
        }
        */
    };

    return {
        likePost: likePost,
        dislikePost: dislikePost,
        handleEnter: handleEnter,
        editCommentAjax: editCommentAjax,
        deleteComment: deleteComment,
        editCommentEvent: editCommentEvent,
        deleteCommentEvent: deleteCommentEvent,
        isTagging: isTagging,
        setTagging: setTagging,
        setListFriend: setListFriend,
        findUserID: findUserID
    }
})();

$(document).ready(function () {
    commentHubProxy = $.connection.commentHub;

    commentHubProxy.client.addNewComment = function (content) {
        $(".comment-list").append(content);
        $(".comment-list .comment-detail").last().find(".commenter-delete").remove();
        $(".comment-list .comment-detail").last().find(".commenter-edit").remove();

        var text = $(".comment-stats").find("span").text().split(" ");
        text[0] = parseInt(text[0]) + 1;
        $(".comment-stats").find("span").text(text.join(" "));
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