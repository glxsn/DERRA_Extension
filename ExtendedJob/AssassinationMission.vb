Imports System
Imports System.Reflection
Imports System.Collections
Imports System.Linq
Imports GTA
Imports GTA.Math
Imports LemonUI.Menus


' 1) 反射找到 DERRA 的菜单实例并在菜单里添加 "开始刺杀任务" 项
' 2) 点击后在玩家附近生成一个持枪目标并为其添加 Blip
' 3) 用 Tick 监控目标死亡/消失并自动清理
' 该实现不继承原版 Mission，避免类型/可见性问题。

Public Class AssassinationMission
    Inherits Script

    '动态生成任务点
    Private Function GetSafeDynamicSpawnPoint() As Vector3
        Dim playerPos As Vector3 = Game.Player.Character.Position
        Dim rnd As New Random()

        For i As Integer = 1 To 20
            Dim radius As Single = rnd.Next(200, 600)        '与玩家距离200-600
            Dim angle As Double = rnd.NextDouble() * Math.PI * 2

            Dim offset As New Vector3(
                radius * Math.Cos(angle),
                radius * Math.Sin(angle),
                0
            )

            Dim pos As Vector3 = playerPos + offset
            Dim roadPos As Vector3 = World.GetNextPositionOnStreet(pos)

            If roadPos.DistanceTo(playerPos) > 150 Then
                Return roadPos
            End If
        Next

        '若20次任然失败
        Return World.GetNextPositionOnStreet(playerPos.Around(300.0F))
    End Function


    Private menuInjected As Boolean = False

    ' 当前进行中的目标与标记
    Private currentTarget As Ped = Nothing
    Private currentBlip As Blip = Nothing

    Public Sub New()
        ' 在脚本加载时（每帧尝试一次）注入菜单项，注入成功后停止尝试
        AddHandler Tick, AddressOf OnTick
    End Sub

    Private Sub OnTick(sender As Object, e As EventArgs)
        Try
            If Not menuInjected Then
                TryInjectMenu()
            End If

            ' 每帧检查当前目标状态，自动清理（如果有）
            If currentTarget IsNot Nothing Then
                If Not currentTarget.Exists OrElse currentTarget.IsDead Then
                    CleanupTarget()
                End If
            End If
        Catch
            ' 静默捕获，不抛给游戏
        End Try
    End Sub

    ' 注入菜单项（反射查找包含 "随机差事生成器" 的菜单并追加项）
    Private Sub TryInjectMenu()
        Try
            ' 在所有已加载程序集里查找可能包含 MissionMenu 的类型（安全、泛化）
            Dim assemblies = AppDomain.CurrentDomain.GetAssemblies()

            For Each asm In assemblies
                Dim types As Type()
                Try
                    types = asm.GetTypes()
                Catch
                    Continue For
                End Try

                For Each t In types
                    ' 查找静态字段或属性类型是 NativeMenu 或派生的
                    Dim fields = t.GetFields(BindingFlags.Static Or BindingFlags.NonPublic Or BindingFlags.Public)
                    For Each f In fields
                        If f Is Nothing Then Continue For
                        If Not GetType(NativeMenu).IsAssignableFrom(f.FieldType) Then Continue For

                        Dim menuInstance As Object = Nothing
                        Try
                            menuInstance = f.GetValue(Nothing)
                        Catch
                        End Try
                        If menuInstance Is Nothing Then Continue For

                        If InjectIntoMenuInstance(menuInstance) Then
                            menuInjected = True
                            Return
                        End If
                    Next

                    ' 再检查静态属性
                    Dim props = t.GetProperties(BindingFlags.Static Or BindingFlags.NonPublic Or BindingFlags.Public)
                    For Each p In props
                        If p Is Nothing Then Continue For
                        If Not GetType(NativeMenu).IsAssignableFrom(p.PropertyType) Then Continue For

                        Dim menuInstance As Object = Nothing
                        Try
                            menuInstance = p.GetValue(Nothing, Nothing)
                        Catch
                        End Try
                        If menuInstance Is Nothing Then Continue For

                        If InjectIntoMenuInstance(menuInstance) Then
                            menuInjected = True
                            Return
                        End If
                    Next
                Next
            Next
        Catch
            ' 忽略错误，留待下一帧重试
        End Try
    End Sub

    ' 尝试在具体的 NativeMenu 实例中追加菜单项
    Private Function InjectIntoMenuInstance(menuInstance As Object) As Boolean
        If menuInstance Is Nothing Then Return False

        Try
            Dim menuType = menuInstance.GetType()

            ' Items 属性应返回 IList
            Dim itemsProp = menuType.GetProperty("Items", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
            If itemsProp Is Nothing Then Return False

            Dim itemsObj = itemsProp.GetValue(menuInstance, Nothing)
            Dim itemsList = TryCast(itemsObj, IList)
            If itemsList Is Nothing Then Return False

            ' 检查此菜单是否包含标题为 "随机差事生成器" 的原项（表明是目标菜单）
            Dim hasRandom As Boolean = False
            For Each it In itemsList
                If it Is Nothing Then Continue For
                Dim titleProp = it.GetType().GetProperty("Title", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
                If titleProp IsNot Nothing Then
                    Dim titleVal = titleProp.GetValue(it, Nothing)
                    If titleVal IsNot Nothing AndAlso titleVal.ToString() = "随机差事生成器" Then
                        hasRandom = True
                        Exit For
                    End If
                End If
            Next
            If Not hasRandom Then Return False

            ' 避免重复添加：若已存在 "开始刺杀任务" 则跳过
            For Each it In itemsList
                If it Is Nothing Then Continue For
                Dim titleProp = it.GetType().GetProperty("Title", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
                If titleProp IsNot Nothing Then
                    Dim titleVal = titleProp.GetValue(it, Nothing)
                    If titleVal IsNot Nothing AndAlso titleVal.ToString() = "开始刺杀任务" Then
                        Return True
                    End If
                End If
            Next

            ' 构造一个新的 NativeItem 并绑定 Activated 事件
            Dim newItem = New NativeItem("开始刺杀任务", "扩展：生成刺杀目标")
            AddHandler newItem.Activated, Sub()
                                              StartAssassination()
                                          End Sub

            ' 如果 menuInstance 有 Add 方法优先使用，否则直接加到 Items 列表
            Dim addMethod = menuType.GetMethod("Add", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
            If addMethod IsNot Nothing Then
                addMethod.Invoke(menuInstance, New Object() {newItem})
            Else
                itemsList.Add(newItem)
            End If

            Return True
        Catch
            Return False
        End Try
    End Function

    ' 点击菜单后执行：生成目标 NPC 并添加 Blip
    Private Sub StartAssassination()
        Try
            ' 若已有目标且存在，则不重复生成
            If currentTarget IsNot Nothing AndAlso currentTarget.Exists Then
                Return
            End If

            Dim pos As Vector3 = GetSafeDynamicSpawnPoint()

            ' 生成目标 Ped（简单安全的选择）
            currentTarget = World.CreatePed(PedHash.Business01AMM, pos)
            If currentTarget Is Nothing Then Return

            ' 给武器（手枪）
            currentTarget.Weapons.Give(WeaponHash.Pistol, 60, True, True)

            ' 添加 Blip 引导玩家
            currentBlip = currentTarget.AddBlip()
            currentBlip.IsShortRange = False
            currentBlip.ShowRoute = True

            ' 确保目标不会立刻消失（标记为不再需要由我们手动清理）
            currentTarget.BlockPermanentEvents = False
            currentTarget.IsPersistent = True

            ' （可选）如果你希望目标主动攻击玩家，可以把下面两行解注释：
            ' World.SetRelationshipBetweenGroups(RelationshipGroup.Hate, Game.Player.Character.RelationshipGroup, currentTarget.RelationshipGroup)
            ' currentTarget.Task.FightAgainst(Game.Player.Character)

        Catch
            ' 静默失败
        End Try
    End Sub

    ' 清理当前目标与 Blip
    Private Sub CleanupTarget()
        Try
            If currentBlip IsNot Nothing Then
                currentBlip.Delete()
                currentBlip = Nothing
            End If
        Catch
        End Try

        Try
            If currentTarget IsNot Nothing AndAlso currentTarget.Exists Then
                currentTarget.MarkAsNoLongerNeeded()
                currentTarget = Nothing
            End If
        Catch
        End Try
    End Sub

End Class
