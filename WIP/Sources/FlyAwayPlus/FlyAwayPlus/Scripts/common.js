var commonModule = (function () {
    // Object: User in model
    var _listFriend = [];
    var _isTagging = false;

    convertTime = function (time) {
        // time format: YYYY/MM/DD HH:mm:ss
        var timeList = time.split(" ");
        var now = new Date();
        var yearTime, monthTime, dayTime, hourTime, minuteTime, secondTime, miliTime, dateTime;
        var year, month, day, hour, minute, second, mili, diff;

        yearTime = parseInt(timeList[0].split("/")[0]);
        monthTime = parseInt(timeList[0].split("/")[1]);
        dayTime = parseInt(timeList[0].split("/")[2]);

        hourTime = parseInt(timeList[1].split(":")[0]);
        minuteTime = parseInt(timeList[1].split(":")[1]);
        secondTime = parseInt(timeList[1].split(":")[2]);
        dateTime = new Date(yearTime, monthTime - 1, dayTime, hourTime, minuteTime, secondTime, 0);
        miliTime = parseInt(dateTime.getMilliseconds());

        year = parseInt(now.getFullYear());
        month = parseInt(now.getMonth() + 1);
        day = parseInt(now.getDate());

        hour = parseInt(now.getHours());
        month = parseInt(now.getMinutes());
        day = parseInt(now.getSeconds());
        mili = parseInt(now.getMilliseconds());

        diff = mili - miliTime;
        // 60s, 1000ms --> Convert 1m to ms
        if (diff < 60 * 1000) {
            return (second - secondTime) + " seconds ago";
        }
            // 60m, 60s, 1000ms --> Convert 1h to ms
        else if (diff < 60 * 60 * 1000) {
            return (minute - minuteTime) + "minutes ago";
        }
            // 24h, 60m, 60s, 1000ms --> Convert 1day to ms
        else if (diff < 24 * 60 * 60 * 1000) {
            return (hour - hourTime) + "hours ago";
        }
        else {
            return time;
        }
    };

    var callAjax = function (controller, parameters, callbackMethod) {
        $.ajax({
            url: controller,
            type: 'POST',
            dataType: 'json',
            data: parameters,
            success: function (data) {
                if (callbackMethod) {
                    eval(callbackMethod);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                if (callbackMethod) {
                    var data = "error";
                    eval(callbackMethod);
                }
            }
        });
    };

    var setBlocksit = function (objBlog, width) {
        objBlog = objBlog ? objBlog : $(".blog-landing");
        width = width ? width : 320;
        if (typeof (objBlog) != "undefined" || objBlog != null || objBlog.length > 0) {
            $(objBlog).each(function (evt) {
                try {
                    var conWidth = $(this).width();
                    var gridwidth = width;

                    var col = conWidth / gridwidth;

                    $(this).BlocksIt({
                        numOfCol: Math.floor(col),
                        offsetX: 8,
                        offsetY: 8
                    });
                }
                catch (e) {
                    console.log("Blocksit: " + e.message);
                }
            });
        }
    };

    var comnpileTagFriend = function (username) {

    };

    var setListFriend = function (newListFriend) {
        _listFriend = newListFriend;
    };

    var getListFriend = function () {
        return _listFriend;
    };

    var findUserId = function (keyword) {
        if (_listFriend.length > 0) {
            for (var index = 0; index < _listFriend.length; index++) {
                var username = _listFriend[index]["FirstName"].toUpperCase() + " " + _listFriend[index]["LastName"].toUpperCase();
                if (username.toUpperCase() == keyword.toUpperCase()) {
                    return _listFriend[index]["UserId"];
                }
            }
        }
        return 0;
    };

    var decompileTagFriend = function (value) {
        var pattern = /@([0-9a-z A-Z]*):/g;
        var tagFriend = value.match(pattern);
        if (tagFriend != null) {
            var username = "";
            var replaceUser = "";

            for (var index = 0; index < tagFriend.length; index++) {
                username = tagFriend[index].substring(1, tagFriend[index].length - 1);
                replaceUser = "<a href='/User/Index/" + findUserId(username) + "'>" + username + "</a>";
                value = value.split(tagFriend[index]).join(replaceUser);
            }
        }
        return value;
    };

    var createTagFriendList = function (keyword) {
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

    var createTagFriendUI = function (keyword, selector) {
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
            tmp += "<img class='lazy member_image thumb_24 member_preview_image' src='" + listFriend[index]["Avatar"] + "'>";
            tmp += "<span class='username'>" + listFriend[index]["FirstName"] + " " + listFriend[index]["LastName"]; + "</span>";
            tmp += "</li>";
            $("#id-tag-friend-list").append(tmp);
        }

        if (listFriend.length > 0) {
            $("#id-tag-friend-tab-ui").show();
            var offset = $(selector).offset();
            var top = offset.top;
            var left = offset.left;
            var width = 0;
            width += parseInt($(selector).css("padding-left").split("px").join(""));
            width += parseInt($(selector).css("margin-left").split("px").join(""));
            width += parseInt($(selector).css("padding-right").split("px").join(""));
            width += parseInt($(selector).css("border-right-width").split("px").join(""));
            width += parseInt($(selector).css("border-left-width").split("px").join(""));

            top = top - 1 - $("#id-tag-friend-tab-ui").height();
            $("#id-tag-friend-tab-ui").offset({ top: top, left: left });
            $("#id-tag-friend-tab-ui").width(parseInt($(selector).width()) + width);
            
            
            $(".tab_complete_ui_item").click(function (evt) {
                if (evt.handled !== true) { // This will prevent event triggering more then once
                    // do anything?
                    var username = $(this).find(".username").text();
                    var currentText = $(selector).val().slice(0, $(selector).val().lastIndexOf('@') + 1);
                    $(selector).val(currentText + username + ": ");
                    $(selector).focus();
                    $("#id-tag-friend-tab-ui").hide();

                    evt.handled = true;
                    evt.preventDefault();
                }
            });
        }
    };

    var handleTagging = function (evt, selector) {
        if (evt.keyCode == 50 && evt.shiftKey) {
            // input @ character
            if (_isTagging == false) {
                _isTagging = true;
                createTagFriendUI("", selector);
            }
        }
        else if (evt.keyCode == 8) {
            // input "backspace" character
            if (_isTagging == true) {
                if ($(selector).val().length > 0 && $(selector).val().slice(-1) == '@') {
                    _isTagging = false;
                    $("#id-tag-friend-tab-ui").hide();
                }
                else {
                    var keyword = $(selector).val().substring($(selector).val().lastIndexOf("@") + 1, $(selector).val().length - 1);
                    createTagFriendUI(keyword, selector);
                }
            }
        }
        else {
            if (_isTagging == true) {
                var keyword = $(selector).val().substring($(selector).val().lastIndexOf("@") + 1, $(selector).val().length) + String.fromCharCode((96 <= evt.keyCode && evt.keyCode <= 105) ? evt.keyCode - 48 : evt.keyCode);
                createTagFriendUI(keyword, selector);
            }
        }
    };

    var findListFriendIdTagging = function (value) {
        var listFriendId = [];
        var pattern = /@([0-9a-z A-Z]*):/g;
        var tagFriend = value.match(pattern);
        if (tagFriend != null) {
            var username = "";
            for (var index = 0; index < tagFriend.length; index++) {
                username = tagFriend[index].substring(1, tagFriend[index].length - 1);
                listFriendId.push(findUserId(username));
            }
        }
        return listFriendId;
    }

    var likePost = function () {
        $(".room-detail-post-list .btn-like").each(function (e) {
            $(this).click(function (evt) {
                if (evt.handled !== true) { // This will prevent event triggering more then once
                    evt.handled = true;
                    var likeIcon = $(this).closest(".panel-body").find(".fa-thumbs-o-up");

                    likeIcon.toggleClass("interacted")
                            .toggleClass("fa-thumbs-up");

                    var likeCountElement = $(this).closest(".panel-body").find(".like-count");

                    if (likeIcon.hasClass("interacted")) {
                        likeCountElement.text(parseInt(likeCountElement.text()) + 1);
                    } else {
                        likeCountElement.text(parseInt(likeCountElement.text()) - 1);
                    }

                    unDislikePost(this);
                    likeAjax(this);
                }
            });
        })
    };

    var dislikePost = function () {
        $(".room-detail-post-list .btn-dislike").each(function (e) {
            $(this).click(function (evt) {
                if (evt.handled !== true) { // This will prevent event triggering more then once
                    evt.handled = true;
                    var dislikeIcon = $(this).closest(".panel-body").find(".fa-thumbs-o-down");

                    dislikeIcon.toggleClass("interacted")
                            .toggleClass("fa-thumbs-down");

                    var dislikeCountElement = $(this).closest(".panel-body").find(".dislike-count");

                    if (dislikeIcon.hasClass("interacted")) {
                        dislikeCountElement.text(parseInt(dislikeCountElement.text()) + 1);
                    } else {
                        dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
                    }

                    unLikePost(this);
                    dislikeAjax(this);
                }
            });
        });
    };

    var unLikePost = function (selector) {
        var likeIcon = $(selector).closest(".panel-body").find(".fa-thumbs-o-up");

        var likeCountElement = $(selector).closest(".panel-body").find(".like-count");

        if (likeIcon.hasClass("interacted")) {
            likeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-up");
            likeCountElement.text(parseInt(likeCountElement.text()) - 1);
        }
    };

    var unDislikePost = function (selector) {
        var dislikeIcon = $(selector).closest(".panel-body").find(".fa-thumbs-o-down");

        var dislikeCountElement = $(selector).closest(".panel-body").find(".dislike-count");

        if (dislikeIcon.hasClass("interacted")) {
            dislikeIcon.toggleClass("interacted")
                    .toggleClass("fa-thumbs-down");
            dislikeCountElement.text(parseInt(dislikeCountElement.text()) - 1);
        }
    };

    var likeAjax = function (post) {
        var controller = "/User/Like";
        var postId = parseInt($(post).attr("role"));
        var data = {
            postId: postId
        }

        commonModule.callAjax(controller, data, null);
    };

    var dislikeAjax = function (post) {
        var controller = "/User/Dislike";
        var postId = parseInt($(post).attr("role"));
        var data = {
            postId: postId
        }

        commonModule.callAjax(controller, data, null);
    };

    var commentAjax = function (post, postId, roomId, roomProxy) {
        var controller = "/User/Comment";
        //var postId = parseInt($(post).attr("role"));
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
                var value = data;
                var pattern = /@@([0-9a-z A-Z]*):/g;
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

                $(post).closest(".comment-area").find(".comment-list").append(value);
                $(post).text("").val("");

                deleteCommentEvent();
                editCommentEventInRoom();
                roomProxy.server.sendCommentInRoom(value, postId, roomId);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }

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
                    cmtDeleteSouce.closest(".comment-detail").remove();
                    commonModule.deleteComment(commentId);
                    evt.preventDefault();
                }

                swal({ title: "Deleted!", text: "Your comment has been deleted.", type: "success", timer: 1500, showConfirmButton: false });
            });
        });
    };

    var isTaggingInRoom = false;
    // postId, roomId, roomProxy
    var handleEnterComment = function (evt, selector, param, mode) {
        if (evt.keyCode == 13 && evt.shiftKey) {
            if (evt.type == "keydown") {
                // create new line
                $(this).val($(this).val() + "\n");
            }
            evt.preventDefault();
        }
        else if (evt.keyCode == 50 && evt.shiftKey) {
            isTaggingInRoom = true;
            // input @@ character
            commonModule.createTagFriendUI("", selector);
        }
        else if (evt.keyCode == 13) {
            if (mode) {
                if ($(selector).val().trim().length > 0) {
                    editCommentAjax($(selector));
                    var parent = $(selector).closest(".comment-content");
                    $(selector).hide();

                    var value = $(selector).val().split("<br />").join("\n");
                    var pattern = /@@([0-9a-z A-Z]*):/g;
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
                if ($(selector).val() != "") {
                    commentAjax(selector, param.PostId, param.RoomId, param.roomProxy);
                }
            }
            evt.preventDefault();
        }
        else if (evt.keyCode == 8) {
            var obj = selector;
            if (isTaggingInRoom == true) {
                if ($(obj).val().length > 0 && $(obj).val().slice(-1) == '@@') {
                    isTaggingInRoom = false;
                    $("#id-tag-friend-tab-ui").hide();
                }
                else {
                    var keyword = $(obj).val().substring($(obj).val().lastIndexOf("@@") + 1, $(obj).val().length - 1);
                    commonModule.createTagFriendUI(keyword, selector);
                }
            }
        } else {
            if (isTaggingInRoom == true) {
                var obj = selector;
                var keyword = $(obj).val().substring($(obj).val().lastIndexOf("@@") + 1, $(obj).val().length) + String.fromCharCode((96 <= evt.keyCode && evt.keyCode <= 105) ? evt.keyCode - 48 : evt.keyCode);
                commonModule.createTagFriendUI(keyword, selector);
            }
        }
    };

    return {
        convertTime: convertTime,
        callAjax: callAjax,
        setBlocksit: setBlocksit,
        comnpileTagFriend: comnpileTagFriend,
        setListFriend: setListFriend,
        getListFriend: getListFriend,
        decompileTagFriend: decompileTagFriend,
        createTagFriendUI: createTagFriendUI,
        findListFriendIdTagging: findListFriendIdTagging,
        handleTagging: handleTagging,
        likePost: likePost,
        dislikePost: dislikePost,
        commentAjax: commentAjax,
        editCommentAjax: editCommentAjax,
        deleteComment: deleteComment,
        deleteCommentEvent: deleteCommentEvent
    }
})();

$(document).ready(function () {
    //commonModule.clickUploadPhoto();
});
