/*
var commonModule = (function () {
    clickUploadPhoto = function () {
        $("#idBtnUploadPhoto").click(function (e) {
            if (e.handled !== true) {
                alert('a');
                e.handled = true;
            }
        });
    };
    

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

    return {
        convertTime: convertTime
    }
})();
*/
$(document).ready(function () {
    //commonModule.clickUploadPhoto();
});
