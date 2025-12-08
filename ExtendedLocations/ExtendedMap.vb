Imports GTA
Imports GTA.Math
Imports System.Reflection

Namespace DERRA.Extension.ExtendedLocations
    ''' <summary>
    ''' 扩展任务地点 - 自动注入到原版 DERRA
    ''' </summary>
    Public Class MapExtension
        Inherits Script

        Private Shared injected As Boolean = False
        Private firstTick As Boolean = False
        Private notificationTime As Integer = 0

        Public Sub New()
            ' 脚本加载时自动注入
            If Not injected Then
                InjectMissionPoints()
                injected = True
            End If

            AddHandler Tick, AddressOf OnTick
        End Sub

        Private Sub OnTick(sender As Object, e As EventArgs)
            If Not firstTick Then
                firstTick = True
                notificationTime = Game.GameTime + 1500
                Return
            End If

            If notificationTime > 0 AndAlso Game.GameTime >= notificationTime Then
                notificationTime = 0                                                  '防止重复
                UI.Notification.PostTicker("~g~DERRA Extension 已激活", True)
            End If

        End Sub

        ''' <summary>
        ''' 注入扩展任务地点到原版 Map
        ''' </summary>
        Private Sub InjectMissionPoints()
            Try
                ' 获取原版 Map 类
                Dim mapType = Type.GetType("DERRA.Map, DERRA")
                If mapType Is Nothing Then
                    UI.Notification.PostTicker("~r~DERRA Extension: 未找到原版 Map 类", True) '过时的引用替换为此版本推荐的
                    Return
                End If

                ' 通过反射获取私有字段 mission_points
                Dim missionPointsField = mapType.GetField("mission_points",
                    BindingFlags.NonPublic Or BindingFlags.Static)

                If missionPointsField Is Nothing Then
                    UI.Notification.PostTicker("~r~DERRA Extension: 无法访问 mission_points", False) '过时的引用替换为此版本推荐的
                    Return
                End If

                ' 获取原有的任务地点列表
                Dim originalPoints = DirectCast(
                    missionPointsField.GetValue(Nothing),
                    List(Of Vector3)
                )

                If originalPoints Is Nothing Then
                    UI.Notification.PostTicker("~r~DERRA Extension: mission_points 为空", True) '过时的引用替换为此版本推荐的
                    Return
                End If

                ' 记录原有地点数量
                Dim originalCount = originalPoints.Count

                ' 添加扩展地点
                AddExtendedPoints(originalPoints)

                ' 确认注入成功
                Dim newCount = originalPoints.Count
                UI.Notification.PostTicker($"~g~DERRA Extension 已加载~n~" &
                                    $"~w~原有地点: {originalCount}~n~" &
                                    $"~w~新增地点: {newCount - originalCount}~n~" &
                                    $"~w~总计地点: {newCount}", True)                                 '过时的引用替换为此版本推荐的

            Catch ex As Exception
                UI.Notification.PostTicker($"~r~DERRA Extension 加载失败:~n~{ex.Message}", False)     '过时的引用替换为此版本推荐的
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
            'points.Add(New Vector3(120.5, -1281.5, 29.5))      ' 警察局
            points.Add(New Vector3(126.0, -1929.0, 21.4))      ' 体育馆
            'points.Add(New Vector3(461.0, -988.0, 43.7))       ' 医院
            'points.Add(New Vector3(-59.0, -616.0, 37.0))       ' FIB大楼

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

            '再添加的地点
            ' === 市中心区域（Downtown Los Santos）===
            points.Add(New Vector3(120.5, -1281.5, 29.5))      ' 警察局前广场
            points.Add(New Vector3(126.0, -1929.0, 21.4))      ' 体育馆停车场
            points.Add(New Vector3(461.0, -988.0, 43.7))       ' 医院急诊入口
            points.Add(New Vector3(-59.0, -616.0, 37.0))       ' FIB大楼停车场
            points.Add(New Vector3(215.0, -810.0, 30.7))       ' Legion Square
            points.Add(New Vector3(-72.0, -1102.0, 26.4))      ' Pacific Bluffs
            points.Add(New Vector3(-265.0, -957.0, 31.2))      ' 汽车旅馆
            points.Add(New Vector3(296.0, -585.0, 43.3))       ' Pillbox Hill 街道

            ' === 西海岸区域（West Coast）===
            points.Add(New Vector3(-1305.0, -394.0, 36.7))     ' Eclipse塔楼停车场
            points.Add(New Vector3(-1389.0, -588.0, 30.3))     ' 游艇码头入口
            points.Add(New Vector3(-1800.0, -500.0, 11.8))     ' 海滩停车场
            points.Add(New Vector3(-1117.0, -1559.0, 4.4))     ' 海边仓库空地
            points.Add(New Vector3(-1652.0, -1075.0, 13.0))    ' Del Perro 码头
            points.Add(New Vector3(-1497.0, -658.0, 29.0))     ' Morningwood 大道
            points.Add(New Vector3(-1214.0, -401.0, 34.0))     ' Vespucci 运河
            points.Add(New Vector3(-1035.0, -476.0, 36.5))     ' Rockford Hills 入口

            ' === 工业区（Industrial）===
            points.Add(New Vector3(896.0, -1045.0, 32.0))      ' La Mesa 工厂空地
            points.Add(New Vector3(1219.0, -1381.0, 35.4))     ' 废弃仓库广场
            points.Add(New Vector3(819.0, -2157.0, 29.6))      ' 港口货运停车场
            points.Add(New Vector3(-595.0, -1614.0, 33.0))     ' 废弃汽车厂空地
            points.Add(New Vector3(713.0, -966.0, 30.4))       ' La Mesa 街区
            points.Add(New Vector3(1207.0, -617.0, 66.4))      ' Mirror Park 大道
            points.Add(New Vector3(941.0, -1470.0, 31.0))      ' Cypress Flats
            points.Add(New Vector3(-356.0, -2437.0, 6.0))      ' Terminal 港口区

            ' === 豪宅区（Vinewood Hills / Richman）===
            points.Add(New Vector3(2571.0, 294.0, 108.7))      ' 豪宅区山顶空地
            points.Add(New Vector3(-1521.0, 851.0, 181.5))     ' 麦德拉索豪宅车道
            points.Add(New Vector3(-2072.0, 3132.0, 32.8))     ' 山顶观景台
            points.Add(New Vector3(736.0, 1283.0, 360.3))      ' 观景点平台
            points.Add(New Vector3(-1289.0, 449.0, 97.9))      ' Richman 别墅区
            points.Add(New Vector3(-854.0, 682.0, 148.5))      ' West Vinewood 山路
            points.Add(New Vector3(372.0, 409.0, 145.7))       ' Vinewood Hills 观景台
            points.Add(New Vector3(-564.0, 286.0, 82.2))       ' Richman Glen 空地

            ' === 沙漠地带（Grand Senora Desert）===
            points.Add(New Vector3(1692.0, 3866.0, 34.9))      ' 黄沙快递站停车场
            points.Add(New Vector3(1983.0, 3053.0, 47.2))      ' 峡谷空地
            points.Add(New Vector3(1729.0, 4676.0, 43.0))      ' 黄沙海岸
            points.Add(New Vector3(2522.0, -383.0, 92.9))      ' 监狱观景台空地
            points.Add(New Vector3(264.0, 2594.0, 44.8))       ' 沙漠公路休息站
            points.Add(New Vector3(1702.0, 3290.0, 41.1))      ' Grand Senora 加油站
            points.Add(New Vector3(2433.0, 4969.0, 46.8))      ' Grapeseed 主干道
            points.Add(New Vector3(1395.0, 3618.0, 38.9))      ' Sandy Shores 镇中心
            points.Add(New Vector3(1905.0, 4925.0, 48.8))      ' Grapeseed 农场区
            points.Add(New Vector3(2683.0, 3453.0, 55.8))      ' 沙漠山丘

            ' === 北部地区（Paleto Bay / Blaine County）===
            points.Add(New Vector3(-1117.0, 2697.0, 18.5))     ' 葡萄籽农场空地
            points.Add(New Vector3(1413.0, 1119.0, 114.8))     ' 葡萄籽山区平台
            points.Add(New Vector3(-3086.0, 658.0, 11.6))      ' 帕利托湾海滩
            points.Add(New Vector3(-437.0, 6161.0, 31.5))      ' 帕利托湾镇中心广场
            points.Add(New Vector3(1861.0, 4706.0, 38.0))      ' 格雷普籽主干道
            points.Add(New Vector3(-378.0, 6062.0, 31.5))      ' Paleto Bay 警察局
            points.Add(New Vector3(-234.0, 6318.0, 31.5))      ' Paleto Bay 商业街
            points.Add(New Vector3(1579.0, 6450.0, 25.3))      ' Mount Chiliad 山脚
            points.Add(New Vector3(-1562.0, 5160.0, 19.8))     ' 北部海岸线
            points.Add(New Vector3(-1025.0, 4842.0, 274.6))    ' 北部山区观景点

            ' === 机场及南部区域（Airport / South LS）===
            points.Add(New Vector3(-1200.0, -3100.0, 13.9))    ' 机场货运区停车场
            points.Add(New Vector3(-1042.0, -2750.0, 21.4))    ' 机场停机坪外围
            points.Add(New Vector3(-1336.0, -3044.0, 13.9))    ' 机场南入口空地
            points.Add(New Vector3(-1158.0, -2645.0, 13.8))    ' 机场西侧停车场
            points.Add(New Vector3(-72.0, -2526.0, 6.0))       ' Elysian Island 入口
            points.Add(New Vector3(586.0, -2794.0, 6.0))       ' Terminal 集装箱区空地
            points.Add(New Vector3(1209.0, -2956.0, 5.9))      ' Cypress Flats 南部

            ' === Vespucci / Beach 区域 ===
            points.Add(New Vector3(-1223.0, -1491.0, 4.3))     ' Vespucci 海滩步道
            points.Add(New Vector3(-1375.0, -1270.0, 4.5))     ' 海滨停车场
            points.Add(New Vector3(-1918.0, -569.0, 11.8))     ' Puerto Del Sol 码头
            points.Add(New Vector3(-1726.0, -230.0, 54.5))     ' Pacific Bluffs 山坡

            ' === Downtown Vinewood 区域 ===
            points.Add(New Vector3(185.0, 187.0, 105.5))       ' Vinewood 剧院区
            points.Add(New Vector3(-186.0, 293.0, 93.8))       ' Vinewood 山路
            points.Add(New Vector3(275.0, -345.0, 44.9))       ' Pillbox Hill 商业区
            points.Add(New Vector3(-530.0, -206.0, 37.6))      ' Little Seoul

            ' === East Los Santos 区域 ===
            points.Add(New Vector3(1137.0, -1337.0, 34.8))     ' El Burro Heights
            points.Add(New Vector3(1230.0, -1590.0, 53.8))     ' El Burro Heights 山坡
            points.Add(New Vector3(976.0, -2159.0, 30.5))      ' Terminal 铁路区
            points.Add(New Vector3(893.0, -2284.0, 30.5))      ' Banning 工业区

            ' === Mission Row / Textile City 区域 ===
            points.Add(New Vector3(437.0, -996.0, 30.7))       ' Mission Row 警察局侧门
            points.Add(New Vector3(334.0, -1315.0, 32.5))      ' Strawberry 街区
            points.Add(New Vector3(-215.0, -1456.0, 31.2))     ' Little Seoul 工业区

            ' === Additional Strategic Locations ===
            points.Add(New Vector3(-3152.0, 1089.0, 20.7))     ' Great Ocean Highway 北段
            points.Add(New Vector3(-2196.0, 4289.0, 49.2))     ' Tongva Valley
            points.Add(New Vector3(2537.0, 2594.0, 37.9))      ' Harmony 镇
            points.Add(New Vector3(1967.0, 3816.0, 32.2))      ' Sandy Shores 郊区
            points.Add(New Vector3(-785.0, 5400.0, 34.2))      ' Paleto Forest 林地
            points.Add(New Vector3(2938.0, 4627.0, 48.5))      ' Alamo Sea 湖畔
        End Sub
    End Class
End Namespace