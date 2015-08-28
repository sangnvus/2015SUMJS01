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

        $(objBlog).each(function (evt) {
            var conWidth = $(this).width();
            var gridwidth = width;

            var col = conWidth / gridwidth;

            $(this).BlocksIt({
                numOfCol: Math.floor(col),
                offsetX: 8,
                offsetY: 8
            });
        });
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
                    console.log(_listFriend[index]["UserId"]);
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
            top = top - 1 - $("#id-tag-friend-tab-ui").height();
            $("#id-tag-friend-tab-ui").offset({ top: top, left: left }).width($(selector).width());

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

    return {
        convertTime: convertTime,
        callAjax: callAjax,
        setBlocksit: setBlocksit,
        comnpileTagFriend: comnpileTagFriend,
        setListFriend: setListFriend,
        getListFriend: getListFriend,
        decompileTagFriend: decompileTagFriend,
        createTagFriendUI: createTagFriendUI,
        handleTagging: handleTagging
    }
})();

$(document).ready(function () {
    //commonModule.clickUploadPhoto();
});
