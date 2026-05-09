using Babbelite.Client;

var connection = new BabbeliteClient(true);

while (connection.ConnectionCount == 0)
    await Task.Delay(100);

var translated = await connection.TranslateText("Mám příliš mnoho jablek", "cs", "en");

Console.WriteLine(translated);
