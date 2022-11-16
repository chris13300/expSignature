Module modMain

    Sub Main()
        Dim chaine As String, tabChaine() As String, tabTmp() As String, i As Integer, signatures As String
        Dim fichierINI As String, fichierEXP As String, moteurEXP As String

        Console.Title = My.Computer.Name

        If My.Computer.FileSystem.GetFileInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "Documents\Visual Studio 2013\Projects\expSignature\expSignature\bin\Debug\expSignature.exe").LastWriteTime > My.Computer.FileSystem.GetFileInfo(My.Application.Info.AssemblyName & ".exe").LastWriteTime Then
            MsgBox("Il existe une version plus récente de ce programme !", MsgBoxStyle.Information)
            End
        End If

        fichierINI = My.Computer.Name & ".ini"
        moteurEXP = "D:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\20T Eman 8.20 x64 PCNT.exe"
        If My.Computer.Name = "BOIS" Or My.Computer.Name = "HTPC" Or My.Computer.Name = "TOUR-COURTOISIE" Then
            moteurEXP = "D:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\20T Eman 8.20 x64 BMI2.exe"
        ElseIf My.Computer.Name = "BUREAU" Or My.Computer.Name = "WORKSTATION" Then
            moteurEXP = "E:\JEUX\ARENA CHESS 3.5.1\Engines\Eman\20T Eman 8.20 x64 BMI2.exe"
        End If

        If My.Computer.FileSystem.FileExists(fichierINI) Then
            chaine = My.Computer.FileSystem.ReadAllText(fichierINI)
            If chaine <> "" Then
                tabChaine = Split(chaine, vbCrLf)
                For i = 0 To UBound(tabChaine)
                    If tabChaine(i) <> "" And InStr(tabChaine(i), " = ") > 0 Then
                        tabTmp = Split(tabChaine(i), " = ")
                        If tabTmp(0) <> "" And tabTmp(1) <> "" Then
                            If InStr(tabTmp(1), "//") > 0 Then
                                tabTmp(1) = Trim(gauche(tabTmp(1), tabTmp(1).IndexOf("//") - 1))
                            End If
                            Select Case tabTmp(0)
                                Case "moteurEXP"
                                    moteurEXP = Replace(tabTmp(1), """", "")
                                Case Else

                            End Select
                        End If
                    End If
                Next
            End If
        End If
        My.Computer.FileSystem.WriteAllText(fichierINI, "moteurEXP = " & moteurEXP, False)
        moteur_court = Replace(nomFichier(moteurEXP), ".exe", "")

        fichierEXP = Replace(Command(), """", "")
        If fichierEXP <> "" Then
            If InStr(fichierEXP, "\", CompareMethod.Text) = 0 Then
                fichierEXP = Replace(moteurEXP, nomFichier(moteurEXP), "") & fichierEXP
            End If
            If My.Computer.FileSystem.FileExists(fichierEXP) Then
                If Not expV2(fichierEXP) Then
                    MsgBox(nomFichier(fichierEXP) & " <> experience format v2 !?", MsgBoxStyle.Exclamation)
                    End
                End If
            Else
                Try
                    My.Computer.FileSystem.WriteAllText(fichierEXP, "SugaR Experience version 2", False, New System.Text.UTF8Encoding(False))
                Catch ex As Exception

                End Try
            End If

            Console.WriteLine("1. Sign the EXP file")
            Console.WriteLine("2. Find the signature of the EXP file")
            chaine = Console.ReadLine()

            If chaine = "1" Or chaine = "2" Then
                Console.Write(vbCrLf & "Loading " & moteur_court & "... ")
                chargerMoteur(moteurEXP, fichierEXP)
                Console.WriteLine("OK" & vbCrLf)
                Console.WriteLine(entete & vbCrLf)

                signatures = trouverSignatures(fichierEXP)
                If chaine = "1" Then
                    If signatures = "" Then
                        Console.WriteLine("Enter your signature :")
                        Console.WriteLine("accepted chars : a-z, 0-9, @, %, &, *, (), []") 'les traits, tirets, espaces et autres seront comptés comme des espaces
                        signerEXP(Console.ReadLine(), fichierEXP)
                        signatures = trouverSignatures(fichierEXP)
                    Else
                        If signatures.Split(vbCrLf).Length > 2 Then
                            Console.WriteLine("This EXP file was merged !")
                        Else
                            Console.WriteLine("This EXP file was already signed !")
                        End If
                    End If
                    Console.WriteLine("Signed by : ")
                    Console.WriteLine(signatures)
                ElseIf chaine = "2" Then
                    If signatures = "" Then
                        Console.WriteLine("Not signed yet !")
                    Else
                        If signatures.Split(vbCrLf).Length > 2 Then
                            Console.WriteLine("This EXP file was merged !")
                        End If
                        Console.WriteLine("Signed by : ")
                        Console.WriteLine(signatures)
                    End If
                End If
                dechargerMoteur()
            End If

            Console.WriteLine(vbCrLf & "Press ENTER to close this window.")
            Console.ReadLine()

        End If
    End Sub

    Private Sub signerEXP(signature As String, fichier As String)
        Dim tabChar() As Char, i As Integer, epd As String, tabEXP(23) As Byte

        tabChar = signature.ToCharArray
        For i = 0 To UBound(tabChar)
            epd = charToEPD(tabChar(i), i + 1)

            entreeEXP(tabEXP, epd, bestmove(epd), 0, 100, 245, 1, entree, sortie, moteur_court)

            My.Computer.FileSystem.WriteAllBytes(fichier, tabEXP, True)
        Next

        'sauver cette signature
        If My.Computer.FileSystem.FileExists("signatures.lst") Then
            If InStr(My.Computer.FileSystem.ReadAllText("signatures.lst"), signature & ";", CompareMethod.Text) = 0 Then
                My.Computer.FileSystem.WriteAllText("signatures.lst", signature & ";", True)
            End If
        Else
            My.Computer.FileSystem.WriteAllText("signatures.lst", signature & ";", False)
        End If

    End Sub

    Private Function trouverSignatures(filename As String) As String
        Dim tabSignatures() As String, signature As String, lstSignatures As String
        Dim tabChar() As Char, i As Integer, j As Integer

        'charger signatures
        signature = ""
        lstSignatures = ""
        If My.Computer.FileSystem.FileExists("signatures.lst") Then
            tabSignatures = Split(My.Computer.FileSystem.ReadAllText("signatures.lst"), ";")
            For i = 0 To UBound(tabSignatures)
                If tabSignatures(i) <> "" Then
                    signature = ""
                    tabChar = tabSignatures(i).ToCharArray
                    For j = 0 To UBound(tabChar)
                        If expListe(charToEPD(tabChar(j), j + 1)) = "" Then
                            Exit For
                        End If
                        signature = signature & tabChar(j)
                    Next
                    If LCase(signature) = LCase(tabSignatures(i)) Then
                        lstSignatures = lstSignatures & signature & vbCrLf
                    End If
                End If
            Next
        End If

        Return lstSignatures
    End Function

    Public Function charToEPD(lettreNumber As Char, indexChar As Integer) As String
        Select Case lettreNumber
            Case "A", "a"
                Return "8/3pP3/2p2P2/1p4P1/kpppPPPK/p6P/p6P/8" & " w - - 0 " & Format(indexChar)

            Case "B", "b"
                Return "8/1KPP4/1P2P3/1PPPP3/1p3p2/1p3p2/1kppp3/8" & " w - - 0 " & Format(indexChar)

            Case "C", "c"
                Return "8/2PPP3/1P3K2/1P6/1p6/1p3k2/2ppp3/8" & " w - - 0 " & Format(indexChar)

            Case "D", "d"
                Return "8/1KPPP3/1P3P2/1P3P2/1p3p2/1p3p2/1kppp3/8" & " w - - 0 " & Format(indexChar)

            Case "E", "e"
                Return "8/1PPPPK2/1P6/1PPP4/1p6/1p6/1ppppk2/8" & " w - - 0 " & Format(indexChar)

            Case "F", "f"
                Return "8/1PPPPK2/1P6/1ppp4/1p6/1p6/1k6/8" & " w - - 0 " & Format(indexChar)

            Case "G", "g"
                Return "8/2PPP3/1P3K2/1P6/1p1kpp2/1p3p2/2ppp3/8" & " w - - 0 " & Format(indexChar)

            Case "H", "h"
                Return "1k4K1/1p4P1/1p4P1/1pppPPP1/1p4P1/1p4P1/1p4P1/8" & " w - - 0 " & Format(indexChar)

            Case "I", "i"
                Return "8/2kpPK2/3pP3/3pP3/3pP3/3pP3/2ppPP2/8" & " w - - 0 " & Format(indexChar)

            Case "J", "j"
                Return "8/4ppp1/5k2/5p2/2K2P2/2P2P2/3PP3/8" & " w - - 0 " & Format(indexChar)

            Case "K", "k"
                Return "8/2P2K2/2P1P3/2PP4/2pp4/2p1p3/2p2k2/8" & " w - - 0 " & Format(indexChar)

            Case "L", "l"
                Return "2K5/2P5/2P5/2P5/2P5/2p5/2pppk2/8" & " w - - 0 " & Format(indexChar)

            Case "M", "m"
                Return "8/k6K/pp4PP/p1p2P1P/p2pP2P/p6P/p6P/8" & " w - - 0 " & Format(indexChar)

            Case "N", "n"
                Return "8/1p4P1/1pp3P1/1p1p2P1/1p2P1P1/1p3PP1/1k4K1/8" & " w - - 0 " & Format(indexChar)

            Case "O", "o"
                Return "8/2PPPP2/1P4P1/1P4P1/1k4K1/1p4p1/2pppp2/8" & " w - - 0 " & Format(indexChar)

            Case "P", "p"
                Return "8/1pPPP3/1p2K3/1pPPP3/1p6/1p6/1p6/1k6" & " w - - 0 " & Format(indexChar)

            Case "Q", "q"
                Return "8/1PPP4/K3P3/R3P3/P2pN3/1PPBp3/5p2/6k1" & " w - - 0 " & Format(indexChar)

            Case "R", "r"
                Return "8/1pppP3/1p2P3/1pPPP3/1pP5/1p1P4/1p2P3/1k3K2" & " w - - 0 " & Format(indexChar)

            Case "S", "s"
                Return "8/2PPPK2/1P6/1P6/2PPpp2/6p1/1kpppp2/8" & " w - - 0 " & Format(indexChar)

            Case "T", "t"
                Return "8/1KPPppk1/3Pp3/3Pp3/3Pp3/3Pp3/3Pp3/8" & " w - - 0 " & Format(indexChar)

            Case "U", "u"
                Return "1k4K1/1p4P1/1p4P1/1p4P1/1p4P1/1p4P1/2ppPP2/8" & " w - - 0 " & Format(indexChar)

            Case "V", "v"
                Return "K6k/P6p/1P4p1/1P4p1/2P2p2/2P2p2/3Pp3/8" & " w - - 0 " & Format(indexChar)

            Case "W", "w"
                Return "K6k/P6p/P6p/1P1Pp1p1/1P1Pp1p1/2P2p2/2P2p2/8" & " w - - 0 " & Format(indexChar)

            Case "X", "x"
                Return "K6k/1P4p1/2P2p2/3Pp3/3pP3/2p2P2/1p4P1/8" & " w - - 0 " & Format(indexChar)

            Case "Y", "y"
                Return "K6k/1P4p1/2P2p2/3Pp3/3Pp3/3Pp3/3Pp3/8" & " w - - 0 " & Format(indexChar)

            Case "Z", "z"
                Return "8/1kppppp1/5p2/4p3/3P4/2P5/1PPPPPK1/8" & " w - - 0 " & Format(indexChar)

            Case "0"
                Return "8/2PPPP2/2P2P2/2P2P2/2K2k2/2p2p2/2pppp2/8" & " w - - 0 " & Format(indexChar)

            Case "1"
                Return "4k3/3pp3/2p1p3/4p3/4P3/4P3/2PPKPP1/8" & " w - - 0 " & Format(indexChar)

            Case "2"
                Return "8/2PPP3/1K3P2/4P3/3p4/2p5/1ppppk2/8" & " w - - 0 " & Format(indexChar)

            Case "3"
                Return "8/3PPP2/2K3P1/4PPP1/6p1/2k3p1/3ppp2/8" & " w - - 0 " & Format(indexChar)

            Case "4"
                Return "4K3/3pP3/2p1P3/1p2P3/pkppPPP1/4P3/4P3/8" & " w - - 0 " & Format(indexChar)

            Case "5"
                Return "8/1PPPPK2/1P6/1PPPpp2/5p2/5p2/1kpppp2/8" & " w - - 0 " & Format(indexChar)

            Case "6"
                Return "8/3PPK2/2P5/2Ppp3/2p2p2/2p2p2/3kp3/8" & " w - - 0 " & Format(indexChar)

            Case "7"
                Return "8/1KPPPPP1/5p2/5p2/4p3/3p4/3p4/2k5" & " w - - 0 " & Format(indexChar)

            Case "8"
                Return "8/2PKP3/1P3P2/2PPP3/1p3p2/1p3p2/2pkp3/8" & " w - - 0 " & Format(indexChar)

            Case "9"
                Return "8/3pk3/2p2p2/2p2p2/3ppP2/5P2/2KPP3/8" & " w - - 0 " & Format(indexChar)

            Case "@"
                Return "8/2ppppN1/1p5P/1p1KPN1P/1p1P1P1P/1p1BPBP1/2brrk2/8" & " w - - 0 " & Format(indexChar)

            Case "%"
                Return "1k6/p1p3P1/1p3P2/4P3/3p2P1/2p2P1P/1p4K1/8" & " w - - 0 " & Format(indexChar)

            Case "&"
                Return "8/3pk3/2p2p2/3pp3/3PP1K1/2P2P2/3PP1P1/8" & " w - - 0 " & Format(indexChar)

            Case "*"
                Return "8/4P3/1P2P2p/2P1P1p1/3PKp2/2P1p1p1/1P2p2p/4k3" & " w - - 0 " & Format(indexChar)

            Case "("
                Return "4K3/3P4/2P5/2P5/2p5/2p5/3p4/4k3" & " w - - 0 " & Format(indexChar)

            Case ")"
                Return "3k4/4p3/5p2/5p2/5P2/5P2/4P3/3K4" & " w - - 0 " & Format(indexChar)

            Case "["
                Return "8/3pk3/3p4/3p4/3P4/3P4/3PK3/8" & " w - - 0 " & Format(indexChar)

            Case "]"
                Return "8/4kp2/5p2/5p2/5P2/5P2/4KP2/8" & " w - - 0 " & Format(indexChar)

            Case Else
                Return "8/8/8/8/8/PPPPpppp/KPPPpppk/8" & " w - - 0 " & Format(indexChar)

        End Select
    End Function

End Module
