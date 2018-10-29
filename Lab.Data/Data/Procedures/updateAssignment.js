function updateAssignment(labId, teamAuth, field, value) {
    var context = getContext();
    var coll = context.getCollection();
    var link = coll.getSelfLink();
    var response = context.getResponse();
    if (!labId) throw new Error('labId is undefined.');
    if (!teamAuth) throw new Error('teamAuth is undefined.');
    if (!field) throw new Error('field is undefined')
    if (!value) throw new Error('value is undefined')

    var query = 'SELECT * FROM LabItems labs WHERE labs.id = ""' + labId + '""';
    var run = coll.queryDocuments(link, query, {}, callback);

    function callback(err, docs) {
        if (err) throw err;
        if (docs.length > 0) {
            var team = null;
            var arr = docs[0].domAssignments;
            for (var teamIndex = 0; teamIndex < arr.length; teamIndex++) {
                if (arr[teamIndex].teamAuth == teamAuth) {
                    team = arr[teamIndex];
                    break;
                }
            }
            if (team == null) {
                throw new Error('The dom assignment was not found.')
            }
            UpdateDoc(docs[0], teamIndex);
        }
        else response.setBody('The document was not found.');
    }

    if (!run) {
        throw new Error('The stored procedure could not be processed.');
    }

    function UpdateDoc(doc, index, team) {
        switch (field) {
            case 'sessionId':
                doc.domAssignments[index].sessionId = value;
                break;
            case 'dnsTxtRecord':
                doc.domAssignments[index].dnsTxtRecord = value;
                break;
        }

        var replace = coll.replaceDocument(doc._self, doc, {}, function (err, newdoc) {
            if (err) throw err;
            response.setBody(newdoc);
        });

        if (!replace) {
            throw new Error('The document could not be updated.');
        }
    }
}