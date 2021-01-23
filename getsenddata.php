<?php

$servername = "localhost";
$database = "";
$username = "";
$password = "";

try
{
    $conn = new PDO("mysql:host=$servername;dbname=$database", $username, $password);
    $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
}
catch(PDOException $e)
{
    // TO DO: figure out how to handle errors properly lol
    // echo "Connection failed: " . $e->getMessage();
    exit;
}

function retrieveLeaderboard($board)
{
    global $conn;

    // Query database
    $sql = "SELECT * FROM {$board} ORDER BY time ASC limit 100";
    $result = $conn->query($sql);

    // Loop through the results and print them out, using "\n" as a delimiter
    $num_results = $result->rowCount();
    for ($i = 0; $i < $num_results; $i++)
    {
        if (!($row = $result->fetch()))
            break;

        echo $row["ID"];
        echo "\n";
        echo $row["username"];
        echo "\n";
        echo $row["kartSkinID"];
        echo "\n";
        echo $row["charSkinID"];
        echo "\n";
        echo $row["time"];
        echo "\n";
    }

    $result = null;
}

function postLeaderboard($board)
{
    global $conn;

    // Get data sent from game
    $name = $_POST['username'];
    $kartid = $_POST['kartSkinID'];
    $skinid = $_POST['charSkinID'];
    $time = $_POST['time'];
    $ghost = $_POST['ghostData'];

    // Create prepared statement
    $statement = $conn->prepare("INSERT INTO {$board} (username, kartSkinID, charSkinID, time, ghostData) VALUES (?, ?, ?, ?, ?)");
    $statement->bindValue(1, $name, PDO::PARAM_STR);
    $statement->bindValue(2, $kartid, PDO::PARAM_INT);
    $statement->bindValue(3, $skinid, PDO::PARAM_INT);
    $statement->bindValue(4, $time, PDO::PARAM_INT);
    $statement->bindValue(5, $ghost, PDO::PARAM_LOB);

    // Insert score into leaderboard database
    $statement->execute();
}

// Check what the request was
if (isset($_POST['retrieve_leaderboard']))
{
    retrieveLeaderboard("track_{$_POST['trackID']}");
}
elseif (isset($_POST['post_leaderboard']))
{
    postLeaderboard("track_{$_POST['trackID']}");
}
elseif (isset($_POST['retrieve_ghostfile']))
{
    global $conn;

    $board = $_POST['trackID'];
    $ID = $_POST['ID'];

    // Get row matching given ID on given leaderboard
    $sql = "SELECT * FROM track_{$board} WHERE ID = {$ID}";
    $result = $conn->query($sql);

    if ($row = $result->fetch())
        echo $row["ghostData"];

    $result = null;
}

$conn = null;

?>