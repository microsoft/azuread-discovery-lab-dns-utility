var subs = [];
$(function () {
    $("#btnAddGroup").on("click", function () {
        $("#NewRG").modal("show");
    });
    $("#btnDeleteGroup").on("click", deleteGroup);
    $("#btnSaveRG").on("click", saveGroup);
    $("#btnRefreshDomains").on("click", refreshDomains);
    $("#btnVerifyRG").on("click", verifyZones);
    $("#btnClose").on("click", loadItem);

    $('#NewRG').on('hide.bs.modal', function () {
        $("#AzureSubscriptionId").val("");
        $("#DNSZoneRG").val("");
        $("#Shared")[0].checked = false;
        $("#DomainList tr:gt(0)").remove();
        $("#collapseOne").collapse('show');
        $("#RGForm").collapse('hide');
    });

    function refreshDomains() {
        var data = $("#groupDetails").data("data");
        SiteUtil.AjaxCall("/api/rgapi/refreshDomains", JSON.stringify(data), function (res) {
            loadItems(res);
            var d = $.grep(res, function (o, i) {
                if (o.azureSubscriptionId == data.azureSubscriptionId) return o;
            });
            loadItem(d[0]);

        }, "POST");
    }

    function saveGroup() {
        var data = SiteUtil.GetDataObject("#RGForm");
        data.DomainList = $("#DomainList").data("zones");

        SiteUtil.AjaxCall("/api/rgapi/saverg", JSON.stringify(data), function (res) {
            $("#NewRG").modal("hide");
            loadItems(res);
        }, "POST");
    }
    function deleteGroup() {
        if (!confirm("Delete this group?")) {
            return;
        }
        var data = $("#groupDetails").data("data");
        SiteUtil.AjaxCall("/api/rgapi/deleterg", JSON.stringify(data), function (res) {
            loadItem(null);
            loadItems(res);
        }, "POST");
    }
    function loadItems(data) {
        $("#groupList li").remove();
        for (x = 0; x < data.length; x++) {
            var item = data[x];
            var sub = getSub(item.azureSubscriptionId);
            var className = "";
            if (sub == null) {
                sub = { displayName: "<b>NA</b>" }
                className = "unlinked";
            }
            $("<li/>")
                .html("<a class='" + className + "' href='#'>" + sub.displayName + "/" + item.dnsZoneRg + "</a>")
                .data("data", item)
                .attr("role", "presentation")
                .on("click", function (e) {
                    var d = $(e.currentTarget).data("data");
                    loadItem(d);
                })
                .appendTo("#groupList");
        }
    }
    function getItems() {
        SiteUtil.AjaxCall("/api/rgapi/getitems", null, function (res) {
            loadItems(res);
        });

    }
    function getDnsZones(subid, rgname, callback) {
        var url = "/api/rgapi/getdnszones?subid=" + subid + "&rgname=" + rgname;
        SiteUtil.AjaxCall(url, null, function (res) {
            callback(res);
        });
    }
    function loadItem(item) {
        $("#groupDetailsInfo").show();
        $("#groupDetails").hide();

        $("#viewDomainList option").remove();
        $("#viewAzureSubscriptionId").html("");
        $("#viewDnsZoneRG").html("");
        $("#viewShared").html("");
        if (item != null) {
            $("#groupDetailsInfo").hide();
            $("#groupDetails").data("data", item).show();
            var sub = getSub(item.azureSubscriptionId);
            if (sub == null) {
                sub = {displayName: "N/A (NO RG ACCESS)"}
            }
            $("#viewAzureSubscriptionId").html(sub.displayName + " (" + item.azureSubscriptionId + ")");
            $("#viewDnsZoneRG").html(item.dnsZoneRg);
            $("#viewCreateDate").html(SiteUtil.UtcToLocal(item.createDate));
            $("#viewOwnerAlias").html(item.ownerAlias);
            $("#viewShared").html(item.shared.toString());
            if (item.domainList.length == 0 || sub == null) {
                $("<option/>").html("(no domains available)").appendTo("#viewDomainList");
            } else {
                for (x = 0; x < item.domainList.length; x++) {
                    $("<option/>").html(item.domainList[x]).appendTo("#viewDomainList");
                }
            }
        }
    }
    function getSubs() {
        SiteUtil.AjaxCall("/api/rgapi/getsubscriptions", null, function (res) {
            subs = res.value;
            getItems();
        });
    }
    function getSub(id) {
        var res = $.grep(subs, function (o, i) {
            return (o.subscriptionId == id);
        });
        if (res.length == 1) {
            return res[0];
        }
    }
    function verifyZones() {
        var subId = $("#AzureSubscriptionId").val();
        var zone = $("#DnsZoneRG").val();
        if (subId.length == 0 || zone.length == 0) {
            SiteUtil.ShowMessage("Please enter a subscription ID and resource group name before attempting validation.", "Form Incomplete", SiteUtil.AlertImages.error);
            return;
        }
        getDnsZones(subId, zone, function (res) {
            $("#DomainList tr:gt(0)").remove();
            $("#btnSaveRG").attr("disabled", "disabled");
            $("#verifyStatus").removeClass().addClass("pending");

            var zones = res.value;
            if (zones == null) {
                $("#verifyStatus").removeClass().addClass("failure").html("Resource group not found. Review and confirm the prerequisites.");
                return;
            }
            var zoneNames = [];
            var ok = true;
            for (x = 0; x < zones.length; x++) {
                zoneNames.push(zones[x].name);
                var tr = $("<tr/>");
                var hasTag = (zones[x].tags["RootLabDomain"] == "true");
                if (!hasTag) ok = false;
                $("<td/>").html(zones[x].name).appendTo(tr);
                var classes = (hasTag) ? "glyphicon glyphicon-ok" : "glyphicon glyphicon-thumbs-down";
                var td = $("<td/>");
                $("<span/>").addClass(classes).appendTo(td);
                td.appendTo(tr);
                tr.appendTo("#DomainList tbody");
            }
            if (ok) {
                $("#btnSaveRG").removeAttr("disabled");
                $("#DomainList").data("zones", zoneNames);
                $("#verifyStatus").removeClass().addClass("success").html("Prerequisites confirmed, save to continue.");
            } else {
                $("#verifyStatus").removeClass().addClass("failure").html('One or more zones is missing a tag - review and confirm the prerequisites.');
            }
        });
    }
    getSubs();
});
