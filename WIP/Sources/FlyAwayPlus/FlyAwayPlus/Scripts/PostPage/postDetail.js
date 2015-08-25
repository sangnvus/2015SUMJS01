var postDetailModule = (function () {
    var _isTagging = false;
    var _listFriend = [];

    var likePost = function () {
        $(".btn-like").click(function (evt) {
            if (evt.handled !== true) { // This will prevent event triggering more then once
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
                evt.handled = true;
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
                evt.handled = true;
            }
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
                var value = data;
                var pattern = /@([0-9a-z A-Z]*):/g;
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

                commentHubProxy.server.sendComment(value);
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
        else if (evt.keyCode == 50 && evt.shiftKey) {
            _isTagging = true;
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
                    var pattern = /@([0-9a-z A-Z]*):/g;
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
                    alert("comment's content must not empty!");
                }
            }
            else {
                if ($("#idTextareaComment").val() != "") {
                    commentAjax($("#idTextareaComment"));
                }
            }
            evt.preventDefault();
        }
        else if (evt.keyCode == 8) {
            var obj = selector ? selector : $("#idTextareaComment");
            if (_isTagging == true) {
                if ($(obj).val().length > 0 && $(obj).val().slice(-1) == '@') {
                    _isTagging = false;
                    $("#id-tag-friend-tab-ui").hide();
                }
                else {
                    var obj = selector ? selector : $("#idTextareaComment");
                    var keyword = $(obj).val().substring($(obj).val().lastIndexOf("@") + 1, $(obj).val().length - 1);
                    console.log(keyword)
                    createTagFriendUI(keyword, selector);
                }
            }
        } else {
            if (_isTagging == true) {
                var obj = selector ? selector : $("#idTextareaComment");
                var keyword = $(obj).val().substring($(obj).val().lastIndexOf("@") + 1, $(obj).val().length) + String.fromCharCode((96 <= evt.keyCode && evt.keyCode <= 105) ? evt.keyCode - 48 : evt.keyCode);
                console.log(keyword);
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
        //var checkMouse = false;
        /*
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
        */
        $(".commenter-edit").click(function (evt) {
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

    deleteCommentEvent = function () {
        $(".commenter-delete").click(function (evt) {
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
                    var commentId = cmtDeleteSouce.attr("role");
                    cmtDeleteSouce.closest(".comment-detail").remove();
                    postDetailModule.deleteComment(commentId);

                    evt.handled = true;
                    evt.preventDefault();
                }

                swal("Deleted!", "Your comment has been deleted.", "success");
            });
        });
    };

    isTagging = function () {
        return _isTagging;
    };

    setTagging = function (value) {
        _isTagging = value;
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
            var username = _listFriend[index]["firstName"].toUpperCase() + " " + _listFriend[index]["lastName"].toUpperCase();
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
            tmp = "<li data-index='" + index + "' class='tab_complete_ui_item'>";
            tmp += "<img class='lazy member_image thumb_24 member_preview_image' src='" + listFriend[index]["avatar"] + "'>";
            tmp += "<span class='username'>" + listFriend[index]["firstName"] + " " + listFriend[index]["lastName"]; + "</span>";
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
                var username = $(this).find(".username").text();
                var currentText = $(obj).val().slice(0, $(obj).val().lastIndexOf('@') + 1);
                $(obj).val(currentText + username + ": ");
                $(obj).focus();
                $("#id-tag-friend-tab-ui").hide();
                _isTagging = false;

                evt.handled = true;
                evt.preventDefault();
            }
        });
    };

    findUserID = function (keyword) {
        for (var index = 0; index < _listFriend.length; index++) {
            var username = _listFriend[index]["firstName"].toUpperCase() + " " + _listFriend[index]["lastName"].toUpperCase();
            if (username.toUpperCase() == keyword.toUpperCase()) {
                return _listFriend[index]["userID"];
            }
        }
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