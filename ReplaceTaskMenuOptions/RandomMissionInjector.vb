'Imports System.Reflection
'Imports DERRA
'Imports GTA
'Imports GTA.Math
'Imports LemonUI.Menus

'反射替换原版mod的随机差事生成器选项，和其他任务的联动出现了问题，先不用
'Namespace DERRA.Extension
'    Public Class RandomMissionInjector
'        Inherits Script

'        Private Shared done As Boolean = False

'        Public Sub New()
'            If Not done Then
'                done = True
'                AddHandler Tick, AddressOf OnTick
'            End If
'        End Sub

'        Private Sub OnTick(sender As Object, e As EventArgs)
'            RemoveHandler Tick, AddressOf OnTick

'            ' 取得 MissionMenu 类型
'            Dim missionMenuType As Type = Type.GetType("DERRA.Menus.MissionMenu")
'            If missionMenuType Is Nothing Then Return

'            ' 取得 menu 字段（private static）
'            Dim menuField = missionMenuType.GetField("menu", BindingFlags.NonPublic Or BindingFlags.Static)
'            If menuField Is Nothing Then Return

'            Dim menuObj = menuField.GetValue(Nothing)
'            If menuObj Is Nothing Then Return

'            ' 获取 Items 列表
'            Dim itemsProp = menuObj.GetType().GetProperty("Items")
'            Dim items = DirectCast(itemsProp.GetValue(menuObj), IList)

'            ' 找到原版 “随机差事生成器”
'            Dim randomItem As Object = Nothing

'            For Each it In items
'                If TypeOf it Is NativeItem Then
'                    Dim titleProp = it.GetType().GetProperty("Title")
'                    If titleProp.GetValue(it).ToString() = "随机差事生成器" Then
'                        randomItem = it
'                        Exit For
'                    End If
'                End If
'            Next

'            If randomItem Is Nothing Then Return

'            ' 追加你的新选项
'            Dim newItem As New NativeItem("开始刺杀任务", "从扩展模组添加的任务")
'            AddHandler newItem.Activated,
'                Sub()
'                    Dim pos As Vector3 = Game.Player.Character.Position + New Vector3(0, 3, 0)
'                    Dim m As New AssassinationMission(pos)
'                    MissionScript.StartMission(m)
'                End Sub

'            ' 添加到原菜单
'            menuObj.GetType().GetMethod("Add").Invoke(menuObj, New Object() {newItem})
'        End Sub

'    End Class
'End Namespace
