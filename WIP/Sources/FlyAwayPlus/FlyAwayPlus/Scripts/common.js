var commonModule = (function () {
    // Object: User in model
    var _listFriend = [];
    var _isTagging = false;
    var mapping = {};


    var convertTime = function (time) {
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

    var isDate = function (txtDate) {
        var currVal = txtDate;
        if (currVal == '')
            return false;

        var rxDatePattern = /^(\d{4})(\-)(\d{1,2})(\-)(\d{1,2})$/; //Declare Regex
        var dtArray = currVal.match(rxDatePattern); // is format OK?

        if (dtArray == null)
            return false;

        //Checks for mm/dd/yyyy format.
        var dtMonth = dtArray[3];
        var dtDay = dtArray[5];
        var dtYear = dtArray[1];

        if (dtMonth < 1 || dtMonth > 12)
            return false;
        else if (dtDay < 1 || dtDay > 31)
            return false;
        else if ((dtMonth == 4 || dtMonth == 6 || dtMonth == 9 || dtMonth == 11) && dtDay == 31)
            return false;
        else if (dtMonth == 2) {
            var isleap = (dtYear % 4 == 0 && (dtYear % 100 != 0 || dtYear % 400 == 0));
            if (dtDay > 29 || (dtDay == 29 && !isleap))
                return false;
        }
        return true;
    }

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
        var pattern = /@([ \S]*?):/g;
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

    var createTagFriendList = function (keyword, list) {
        var listTagging = [];
        if (keyword == "") {
            return list ? list : _listFriend;
        }
        if (list) {
            for (var index = 0; index < list.length; index++) {
                var username = list[index]["FirstName"].toUpperCase() + " " + list[index]["LastName"].toUpperCase();
                if (username.indexOf(keyword.toUpperCase()) == 0) {
                    listTagging.push(list[index]);
                }
            }
        } else {
            for (var index = 0; index < _listFriend.length; index++) {
                var username = _listFriend[index]["FirstName"].toUpperCase() + " " + _listFriend[index]["LastName"].toUpperCase();
                if (username.indexOf(keyword.toUpperCase()) == 0) {
                    listTagging.push(_listFriend[index]);
                }
            }
        }
        return listTagging;
    };

    var createTagFriendUI = function (keyword, selector, list) {
        var listFriend = createTagFriendList(keyword, list);
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
                    var userId = parseInt($(this).attr("role"));
                    mapping[username] = userId;
                    console.log(userId);
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

    var handleTagging = function (evt, selector, list) {
        if (evt.keyCode == 50 && evt.shiftKey) {
            // input @ character
            if (_isTagging == false) {
                _isTagging = true;
                createTagFriendUI("", selector, list);
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
                    createTagFriendUI(keyword, selector, list);
                }
            }
        }
        else {
            if (_isTagging == true) {
                var keyword = $(selector).val().substring($(selector).val().lastIndexOf("@") + 1, $(selector).val().length) + String.fromCharCode((96 <= evt.keyCode && evt.keyCode <= 105) ? evt.keyCode - 48 : evt.keyCode);
                createTagFriendUI(keyword, selector, list);
            }
        }
    };

    var findListFriendIdTagging = function (value) {
        var listFriendId = [];
        var pattern = /@([ \S]*?):/g;
        var tagFriend = value.match(pattern);
        if (tagFriend != null) {
            var username = "";
            for (var index = 0; index < tagFriend.length; index++) {
                username = tagFriend[index].substring(1, tagFriend[index].length - 1);
                //listFriendId.push(findUserId(username));
                listFriendId.push(mapping[username]);
            }
        } else {
            listFriendId.push(mapping[value]);
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

    var isDouble = function(value) {
        var number = new Number(value);
        if (/^[0-9]{0,10}(\.[0-9]{0,5})?$/.test(value) && number > 0) {
            return true;
        }
        return false;
    };

    var sentFriendRequestNow = function(fromUserId, toUserId) {
        $.connection.hub.start().done(function () {
            $.connection.friendHub.server.sendFriendRequest(fromUserId, toUserId);
        });
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
        isDate: isDate,
        isDouble: isDouble,
        sentFriendRequestNow: sentFriendRequestNow
    }
})();

$(document).ready(function () {
    //commonModule.clickUploadPhoto();
});
