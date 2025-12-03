'Imports System
'Imports System.Reflection
'Imports System.Collections
'Imports GTA
'Imports LemonUI.Menus

''  扩展任务注入器：
''  1) 仅在游戏加载后反射一次 DERRA 的菜单
''  2) 在“随机差事生成器”下创建一个“扩展任务”子菜单
''  3) 其他任务脚本只需向 ExtMenuInjector.ExtMenu 添加项即可
''先测试完再使用吧

'Public Class ExtMenuInjector
'    Inherits Script

'    Public Shared ExtMenu As NativeMenu = Nothing     ' 全局扩展任务菜单
'    Private Shared injected As Boolean = False        ' 防止重复注入

'    Public Sub New()
'        AddHandler Tick, AddressOf OnTick
'    End Sub

'    Private Sub OnTick(sender As Object, e As EventArgs)
'        If injected Then Return

'        Try
'            If TryInject() Then
'                injected = True
'            End If
'        Catch
'            ' 静默，不抛错误
'        End Try
'    End Sub


'    ' ===========================================================
'    ' 执行一次反射，找到原 DERRA 菜单实例
'    ' 在“随机差事生成器”下创建新的 "扩展任务" 菜单
'    ' ===========================================================
'    Private Function TryInject() As Boolean
'        Dim assemblies = AppDomain.CurrentDomain.GetAssemblies()

'        For Each asm In assemblies
'            Dim types As Type()
'            Try
'                types = asm.GetTypes()
'            Catch
'                Continue For
'            End Try

'            For Each t In types
'                ' 找 NativeMenu 的静态字段或属性
'                Dim menus As List(Of Object) = GetStaticMenus(t)
'                For Each menuInstance In menus
'                    If InjectIntoMenu(menuInstance) Then
'                        Return True
'                    End If
'                Next
'            Next
'        Next

'        Return False
'    End Function


'    ' ===========================================================
'    ' 获取类型中所有静态的 NativeMenu 字段/属性
'    ' ===========================================================
'    Private Function GetStaticMenus(t As Type) As List(Of Object)
'        Dim result As New List(Of Object)

'        Dim fields = t.GetFields(BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
'        For Each f In fields
'            If GetType(NativeMenu).IsAssignableFrom(f.FieldType) Then
'                Dim v As Object = Nothing
'                Try : v = f.GetValue(Nothing) : Catch : End Try
'                If v IsNot Nothing Then result.Add(v)
'            End If
'        Next

'        Dim props = t.GetProperties(BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
'        For Each p In props
'            If GetType(NativeMenu).IsAssignableFrom(p.PropertyType) Then
'                Dim v As Object = Nothing
'                Try : v = p.GetValue(Nothing, Nothing) : Catch : End Try
'                If v IsNot Nothing Then result.Add(v)
'            End If
'        Next

'        Return result
'    End Function


'    ' ===========================================================
'    ' 在一个具体 NativeMenu 里查找 "随机差事生成器"
'    ' 找到后创建 "扩展任务" 子菜单
'    ' ===========================================================
'    Private Function InjectIntoMenu(menuInstance As Object) As Boolean
'        If menuInstance Is Nothing Then Return False

'        Dim menuType = menuInstance.GetType()

'        ' 读取 Items 列表
'        Dim itemsProp = menuType.GetProperty("Items", BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic)
'        If itemsProp Is Nothing Then Return False

'        Dim itemsObj = itemsProp.GetValue(menuInstance, Nothing)
'        Dim itemsList = TryCast(itemsObj, IList)
'        If itemsList Is Nothing Then Return False

'        ' 找原菜单中的 "随机差事生成器"
'        Dim randomJobMenu As NativeMenu = Nothing

'        For Each it In itemsList
'            If it Is Nothing Then Continue For

'            ' NativeMenu 的标题属性一般叫 Subtitle（具体看原 DERRA 的定义）
'            Dim subProp = it.GetType().GetProperty("Subtitle")
'            If subProp Is Nothing Then Continue For

'            Dim subText = TryCast(subProp.GetValue(it, Nothing), String)
'            If subText = "随机差事生成器" Then
'                randomJobMenu = TryCast(it, NativeMenu)
'                Exit For
'            End If
'        Next

'        If randomJobMenu Is Nothing Then Return False

'        ' 避免重复创建
'        If ExtMenu IsNot Nothing Then Return True

'        ' ---- 创建扩展任务菜单 ----
'        ExtMenu = New NativeMenu("扩展任务", "由你创建的任务扩展")
'        randomJobMenu.Add(ExtMenu)

'        Return True
'    End Function

'End Class
