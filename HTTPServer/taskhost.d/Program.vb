
''' <summary>
''' 计算任务的托管进程
''' </summary>
Module Program

    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function
End Module
