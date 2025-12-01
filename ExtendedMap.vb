Imports GTA
Imports GTA.Math
Imports System.Reflection

Namespace DERRA.Extension
    ''' <summary>
    ''' 扩展任务地点 - 自动注入到原版 DERRA
    ''' </summary>
    Public Class MapExtension
        Inherits Script

        Private Shared injected As Boolean = False

        Public Sub New()
            ' 脚本加载时自动注入
            If Not injected Then
                InjectMissionPoints()
                injected = True
            End If
        End Sub

        ''' <summary>
        ''' 注入扩展任务地点到原版 Map
        ''' </summary>
        Private Sub InjectMissionPoints()
            Try
                ' 获取原版 Map 类（它会在hook加载成功前加载，提示字段无法正常显示）
                Dim mapType = Type.GetType("DERRA.Map, DERRA")
                If mapType Is Nothing Then
                    UI.Notification.Show("~r~DERRA Extension: 未找到原版 Map 类")
                    Return
                End If

                ' 通过反射获取私有字段 mission_points
                Dim missionPointsField = mapType.GetField("mission_points",
                    BindingFlags.NonPublic Or BindingFlags.Static)

                If missionPointsField Is Nothing Then
                    UI.Notification.Show("~r~DERRA Extension: 无法访问 mission_points")
                    Return
                End If

                ' 获取原有的任务地点列表
                Dim originalPoints = DirectCast(
                    missionPointsField.GetValue(Nothing),
                    List(Of Vector3)
                )

                If originalPoints Is Nothing Then
                    UI.Notification.Show("~r~DERRA Extension: mission_points 为空")
                    Return
                End If

                ' 记录原有地点数量
                Dim originalCount = originalPoints.Count

                ' 添加扩展地点
                AddExtendedPoints(originalPoints)

                ' 确认注入成功(一样，它会在hook加载成功前加载，所以这段不会显示)
                Dim newCount = originalPoints.Count
                UI.Notification.Show($"~g~DERRA Extension 已加载~n~" &
                                    $"~w~原有地点: {originalCount}~n~" &
                                    $"~w~新增地点: {newCount - originalCount}~n~" &
                                    $"~w~总计地点: {newCount}")

            Catch ex As Exception
                UI.Notification.Show($"~r~DERRA Extension 加载失败:~n~{ex.Message}")
            End Try
        End Sub

        ''' <summary>
        ''' 添加扩展任务地点
        ''' 根据原版的任务逻辑，在抵达消灭敌人后会生成一个手提箱，
        ''' 但这个拓展可能因任务点的坐标问题，让其生成到奇怪的地方导致无法拾取，
        ''' 目前发现“机场南侧”这个地点有概率如此
        ''' </summary>
        Private Sub AddExtendedPoints(points As List(Of Vector3))
            ' === 市中心区域 ===
            points.Add(New Vector3(120.5, -1281.5, 29.5))      ' 警察局
            points.Add(New Vector3(126.0, -1929.0, 21.4))      ' 体育馆
            points.Add(New Vector3(461.0, -988.0, 43.7))       ' 医院
            points.Add(New Vector3(-59.0, -616.0, 37.0))       ' FIB大楼

            ' === 西海岸区域 ===
            points.Add(New Vector3(-1305.0, -394.0, 36.7))     ' Eclipse塔楼
            points.Add(New Vector3(-1389.0, -588.0, 30.3))     ' 游艇码头
            points.Add(New Vector3(-1800.0, -500.0, 11.8))     ' 海滩停车场
            points.Add(New Vector3(-1117.0, -1559.0, 4.4))     ' 海边仓库

            ' === 工业区 ===
            points.Add(New Vector3(896.0, -1045.0, 32.0))      ' La Mesa工厂
            points.Add(New Vector3(1219.0, -1381.0, 35.4))     ' 废弃仓库
            points.Add(New Vector3(819.0, -2157.0, 29.6))      ' 港口货运区
            points.Add(New Vector3(-595.0, -1614.0, 33.0))     ' 废弃汽车厂

            ' === 豪宅区 ===
            points.Add(New Vector3(2571.0, 294.0, 108.7))      ' 豪宅区高地
            points.Add(New Vector3(-1521.0, 851.0, 181.5))     ' 麦德拉索豪宅
            points.Add(New Vector3(-2072.0, 3132.0, 32.8))     ' 山顶军事设施
            points.Add(New Vector3(736.0, 1283.0, 360.3))      ' 观景台

            ' === 沙漠地带 ===
            points.Add(New Vector3(1692.0, 3866.0, 34.9))      ' 黄沙快递站
            points.Add(New Vector3(1983.0, 3053.0, 47.2))      ' 峡谷地带
            points.Add(New Vector3(1729.0, 4676.0, 43.0))      ' 黄沙海岸
            points.Add(New Vector3(2522.0, -383.0, 92.9))      ' 监狱观景台
            points.Add(New Vector3(264.0, 2594.0, 44.8))       ' 沙漠公路旁

            ' === 北部地区 ===
            points.Add(New Vector3(-1117.0, 2697.0, 18.5))     ' 葡萄籽农场
            points.Add(New Vector3(1413.0, 1119.0, 114.8))     ' 葡萄籽山区
            points.Add(New Vector3(-3086.0, 658.0, 11.6))      ' 帕利托湾海滩
            points.Add(New Vector3(-437.0, 6161.0, 31.5))      ' 帕利托湾镇中心
            points.Add(New Vector3(1861.0, 4706.0, 38.0))      ' 格雷普籽主干道

            ' === 机场周边 ===
            points.Add(New Vector3(-1200.0, -3100.0, 13.9))    ' 机场货运区
            points.Add(New Vector3(-1042.0, -2750.0, 21.4))    ' 机场停机坪
            'points.Add(New Vector3(-910.0, -2980.0, 13.9))     ' 机场南侧
        End Sub
    End Class
End Namespace