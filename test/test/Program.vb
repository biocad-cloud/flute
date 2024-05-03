Imports System

Module Program
    Sub Main(args As String())
        Dim ssfile = Flute.SessionManager.Open("asjkdfshdfjksfs".MD5, "Z:/")

        Console.WriteLine(ssfile.OpenKeyString("abc"))
        Console.WriteLine(ssfile.OpenKeyDouble("abc1"))
        Console.WriteLine(ssfile.OpenKeyInteger("abc2"))

        ssfile.SaveKey("abc", "hello world")
        ssfile.SaveKey("abc1", 111.96)
        ssfile.SaveKey("abc2", 99999)

        Console.WriteLine(ssfile.OpenKeyString("abc"))
        Console.WriteLine(ssfile.OpenKeyDouble("abc1"))
        Console.WriteLine(ssfile.OpenKeyInteger("abc2"))

        ssfile.SaveKey("abc", "hello!")

        Console.WriteLine(ssfile.OpenKeyString("abc"))

        ssfile.SaveKey("abc", "hello world!!!!")

        Console.WriteLine(ssfile.OpenKeyString("abc"))
    End Sub
End Module
