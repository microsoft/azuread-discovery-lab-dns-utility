$(function () {
    $("#btnClose").on("click", function () {
        $("#labList li").removeClass("active");
        setDetail();
        $("#labDetails").data("data", null).css("display", "none");
        $("#labDetailsInfo").css("display", "block");

    });
    $("#btnAddLab").on("click", function () {
        loadLabForm();
    });
    $("#btnEditLab").on("click", function () {
        var data = $("#labDetails").data("data");
        loadLabForm(data);
    });
    $("#btnReport").on("click", function () {
        var id = $("#labDetails").data("data").id;
        location.href = "/Admin/LabReport/" + id;
    });
    $("#labList").on("click", "li:not(.info)", function () {
        $("#labList li").removeClass("active");
        $(this).addClass("active");
        var id = $(this).data("id");
        getLab(id);
    });
    $("#btnSaveLab").on("click", saveLab);
    $("#btnDeleteLab").on("click", deleteLab);
    $("#btnResetTeamAuth").on("click", resetTeamAuth);
    $("#btnResetTxtRecord").on("click", resetTxtRecord);
    $("#btnRemoveDomain").on("click", removeDomain);
    $("#btnResetAllDomains").on("click", resetAllDomains);
    $("#btnLabRefresh").on("click", function () {
        var lab = $("#labDetails").data("data");
        getLab(lab.id);
    });

    function editAssignment(e) {
        var team = $(e.currentTarget).data("data");
        $("#EditTeam").data("teamData", team);
        $("#TeamModalLabel").html(team.domainName + " - Team/Domain Resets");
        $("#resetTeamAuthRes").html(team.teamAuth);
        $("#resetTxtRecordRes").html(team.dnsTxtRecord || "not assigned");
        if (team.assignedTenantId) {
            $("#divShowRemoveDomain").show();
            checkDomain(team, function (res) {
                if (res.ResponseMessage.indexOf("Domain operation failed") > -1) {
                    $("#removeDomainRes").html(res.ResponseMessage);
                } else {
                    var domRes = JSON.parse(res.ResponseMessage);
                    var s = "";
                    for (col in domRes) {
                        if (col.substring(0, 1) == "@")
                            continue;
                        s += "<b>" + col + "</b>: " + domRes[col] + "<br>";
                    }
                    $("#removeDomainRes").html(s);
                }
            });
        } else {
            $("#divShowRemoveDomain").hide();
        }
        $("#EditTeam").modal('show');
    }
    function resetTeamAuth() {
        if (!confirm("Are you sure you want to assign this team a new auth code?"))
            return;

        var dom = $("#EditTeam").data("teamData");
        var lab = $("#labDetails").data("data");
        var teamDto = {
            "Lab": lab,
            "TeamAssignment": dom
        }
        SiteUtil.AjaxCall("/api/Lab/ResetTeamCode", JSON.stringify(teamDto), function (res) {
            SiteUtil.ShowMessage(res.ResponseMessage, "Reset Result", SiteUtil.AlertImages.info);
            $("#resetTeamAuthRes").html(res.ResponseMessage);
            setDetail(res.Settings);
        }, "POST");
    }
    function resetTxtRecord() {
        if (!confirm("Are you sure you want to clear any existing TXT record from DNS for this domain?"))
            return;

        var dom = $("#EditTeam").data("teamData");
        var lab = $("#labDetails").data("data");
        var teamDto = {
            "Lab": lab,
            "TeamAssignment": dom
        }
        SiteUtil.AjaxCall("/api/Lab/ResetTxtAssignment", JSON.stringify(teamDto), function (res) {
            SiteUtil.ShowMessage(res.ResponseMessage, "Reset Result", SiteUtil.AlertImages.info);
            $("#resetTxtRecordRes").html(res.ResponseMessage);
            setDetail(res.Settings);
        }, "POST");
    }
    function checkDomain(team, callback) {
        var lab = $("#labDetails").data("data");
        var teamDto = {
            "Lab": lab,
            "TeamAssignment": team
        }
        SiteUtil.AjaxCall("/api/Lab/CheckDomainAssignment", JSON.stringify(teamDto), callback, "POST");
    }

    function removeDomain() {
        if (!confirm("Are you sure you want to invalidate/unlink this domain from the associated tenant?"))
            return;

        var dom = $("#EditTeam").data("teamData");
        var lab = $("#labDetails").data("data");
        var teamDto = {
            "Lab": lab,
            "TeamAssignment": dom
        }
        SiteUtil.AjaxCall("/api/Lab/UnlinkDomain", JSON.stringify(teamDto), function (res) {
            SiteUtil.ShowMessage(res.ResponseMessage, "Reset Result", SiteUtil.AlertImages.info);
            if (res.ResponseMessage.substring(0, 5) != "ERROR") {
                $("#removeDomainRes").html(res.ResponseMessage);
                setDetail(res.Settings);
            }
        }, "POST");
    }

    function resetAllDomains() {
        if (!confirm("This will reset all TXT records and remove validation for each domain in the session.\n\r\n\rThis is an end of day cleanup operation. Are you sure you want to continue?"))
            return;

        var lab = $("#labDetails").data("data");
        var teamDto = {
            "Lab": lab
        }
        SiteUtil.AjaxCall("/api/Lab/UnlinkAllDomains", JSON.stringify(teamDto), function (res) {
            SiteUtil.ShowMessage(res.ResponseMessage, "Reset Result", SiteUtil.AlertImages.info);
            setDetail(res.Settings);
        }, "POST");
    }

    function deleteLab() {
        if (!confirm("Are you sure you want to delete this lab?"))
            return;

        var data = $("#labDetails").data("data");
        SiteUtil.AjaxCall("/api/Lab/DeleteLab/" + data.id, null, function (res) {
            $("#EditLab").modal('hide');
            loadLabList(res);
            setDetail(null);
        }, "POST");

    }
    function saveLab(data) {
        var action = (($("#LabModalLabel").html() == "Add Lab") ? "AddLab" : "UpdateLab");
        var data = {};
        if (action == "AddLab") {
            data = {
                "primaryInstructor": $("#Instructor").val(),
                "labDate": $("#LabDate").val(),
                "city": $("#City").val(),
            }
        } else {
            data = $("#labDetails").data("data");
            data.primaryInstructor = $("#Instructor").val();
            data.labDate = $("#LabDate").val();
            data.city = $("#City").val();
        }

        data.instructors = $('#Instructors').tokenfield('getTokensList', ',', false, false).split(",");

        SiteUtil.AjaxCall("/api/Lab/" + action, JSON.stringify(data), function (res) {
            $("#EditLab").modal('hide');
            loadLabList(res);
            getLab(data.id);
        }, "POST");
    }

    function loadLabForm(data) {
        $("#LabModalLabel").html((data == null) ? "Add Lab" : "Edit Lab");

        $("#Instructor").val((data == null) ? me : data.primaryInstructor);
        $("#LabDate").val((data == null) ? moment().format("MM/DD/YYYY") : SiteUtil.GetShortDate(data.labDate));
        $("#City").val((data == null) ? "" : data.city);
        if (data != null) {
            $('#Instructors').tokenfield('setTokens', data.instructors);
        } else {
            $("#Instructors").tokenfield('setTokens', []);
        }
        $("#EditLab").modal('show');
    }
    function getLabList() {
        SiteUtil.AjaxCall("/api/Lab/GetLabs", null, function (res) {
            labsList = res;
            loadLabList(res);
        });
    }
    function getLab(id) {
        SiteUtil.AjaxCall("/api/Lab/GetLab/" + id, null, function (res) {
            setDetail(res);
        });
    }
    function setDetail(data) {
        if (data == null || typeof (data) == "undefined") {
            $("#labDetails").data("data", null).css("display", "none");
            $("#labDetailsInfo").css("display", "block");
        } else {
            $("#labDetails").data("data", data).css("display", "block");
            $("#labDetailsInfo").css("display", "none");
        }
        $("#labInstructor").html((data == null) ? "" : data.primaryInstructor);
        $("#labLabDate").html((data == null) ? "" : SiteUtil.GetShortDate(data.labDate));
        $("#labCity").html((data == null) ? "" : data.city);
        $("#labLabCode").html((data == null) ? "" : data.labCode);
        $("#labInstructors").html((data == null) ? "" : data.instructors.join(", "));
        $("#labTeamList tr:not(:first-child)").remove();

        if (data == null) return;
        for (var x = 0; x < data.domAssignments.length; x++) {
            var tr = $("<tr/>");
            $("<td/>").html(data.domAssignments[x].domainName).appendTo(tr);
            $("<td/>").html(data.domAssignments[x].teamAuth).appendTo(tr);
            $("<td/>").html(data.domAssignments[x].dnsTxtRecord || "(not assigned)").appendTo(tr);
            $("<td/>").html(data.domAssignments[x].assignedTenantId || "(not assigned)").appendTo(tr);
            tr.addClass("teamRow")
                .attr("title", "Click to reset the Team Auth and TXT Records")
                .data("data", data.domAssignments[x])
                .on("click", editAssignment)
                .appendTo("#labTeamList");
        }
    }

    function checkLabDate(labDate, callback) {
        SiteUtil.AjaxCall("/api/Lab/CheckLabDate", labDate, function (res) {
            callback(res);
        });
    }
    function getAllLabDates(callback) {
        SiteUtil.AjaxCall("/api/Lab/GetLabDates", null, function (res) {
            callback(res);
        });
    }
    function loadLabList(res) {
        $("#labList li").remove();
        if (res.length == 0) {
            $("<li/>").addClass("info").attr("role", "presentation").html("No labs currently assigned").appendTo("#labList");
            return;
        }
        for (var x = 0; x < res.length; x++) {
            var li = $("<li/>")
                .data("id", res[x].id)
                .attr("role", "presentation")
                .appendTo("#labList");
            $("<a/>")
                .html(SiteUtil.GetShortDate(res[x].labDate) + ": " + res[x].city)
                .css("backgroundColor", (res[x].primaryInstructor == me) ? "rgba(246, 254, 246, 1)" : "aliceblue")
                .appendTo(li);
        }
    }
    $('#datepickerLab').datetimepicker({
        format: 'MM/DD/YYYY',
        allowInputToggle: true
    })
        .on("dp.show", function (e) {
            getAllLabDates(function (arr) {
                var arr2 = [];
                $(arr).each(function () {
                    arr2.push(moment(this));
                });
                $("#datepickerLab").data("DateTimePicker").disabledDates(arr2);
            });
        });
    $('#Instructors')
        .tokenfield({
            createTokensOnBlur: true,
            delimiter: [",", " ", ";"],
            inputType: "email"
        })
        .on('tokenfield:createtoken', function (e) {
            if ($.grep($('#Instructors').tokenfield('getTokens'), function (o) { return (o.value == e.attrs.value); }).length > 0) {
                $('#Instructors').data('bs.tokenfield').$input.val("");
                SiteUtil.ShowMessage('"' + e.attrs.value + '" is already in the list.', 'Duplicate Entry');
                e.preventDefault();
            }
            var re = /\S+@microsoft.com/
            var isValid = re.test(e.attrs.value)
            if (!isValid) {
                e.attrs.value = "";
                SiteUtil.ShowMessage('Make sure you enter the UPN (alias@microsoft.com)', 'Check Value');
                e.preventDefault();
            }
            return true;
        });

    //init
    getLabList();
});