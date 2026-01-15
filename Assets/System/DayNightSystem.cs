using System.Collections.Generic;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    // 视觉状态枚举
    private enum VisualState
    {
        Stable,      // 稳定状态：显示当前阶段的天空盒
        Transition,  // 过渡状态：正在向下一个阶段过渡
        Completed    // 完成状态：月相已变化，准备进入稳定状态
    }

    // 控制方向光的引用
    public Light directionalLight;

    // 太阳和月亮对象及材质接口
    public GameObject sunObject;
    public GameObject moonObject;
    public Material sunMaterial;
    public Material moonMaterial;
    public bool useContinuousMotion = true;
    public float orbitRadius = 100f;
    public Vector3 orbitCenter = Vector3.zero;

    // 存储不同过渡阶段对应的天空盒
    public List<TransitionPhaseSkyboxMapping> transitionPhaseMappings;

    // 存储月相和对应天空盒材质的映射关系（用于兼容旧逻辑）
    public List<MoonPhaseSkyboxMapping> moonPhaseMappings;

    // 用于插值的值（范围在0到1之间）
    float blendedValue = 0.0f;

    // 当前的月相
    private GlobalTimeSystem.MoonPhase currentMoonPhase;

    // 下一阶段的月相
    private GlobalTimeSystem.MoonPhase nextMoonPhase;

    // 用于跟踪月相变化
    private GlobalTimeSystem.MoonPhase lastMoonPhase;

    // 当前过渡阶段
    private TransitionPhaseSkyboxMapping currentTransitionPhase;

    // 过渡速度
    private const float TRANSITION_SPEED = 2.0f;

    // 过渡开始阈值（PhaseProgress达到此值时开始过渡）
    private const float TRANSITION_START_THRESHOLD = 0.7f;

    // 当前视觉状态
    private VisualState visualState = VisualState.Stable;

    // 是否已初始化
    private bool isInitialized = false;

    // 初始化方法
    void Start()
    {
        InitializeSystem();
    }

    // 系统初始化
    private void InitializeSystem()
    {
        if (GlobalTimeSystem.Instance == null)
        {
            Debug.LogWarning("GlobalTimeSystem实例未找到，延迟初始化");
            return;
        }

        // 获取当前月相
        currentMoonPhase = GlobalTimeSystem.Instance.CurrentPhase;
        lastMoonPhase = currentMoonPhase;
        
        // 计算下一阶段
        nextMoonPhase = GetNextMoonPhase(currentMoonPhase);
        
        // 根据当前PhaseProgress决定初始状态
        float progress = GlobalTimeSystem.Instance.PhaseProgress;
        
        if (progress >= TRANSITION_START_THRESHOLD)
        {
            // 如果进度已经超过阈值，直接进入过渡状态
            visualState = VisualState.Transition;
            StartTransition();
        }
        else
        {
            // 否则进入稳定状态
            visualState = VisualState.Stable;
            UpdateSkybox(); // 立即设置当前阶段的天空盒
        }
        
        // 初始化太阳和月亮对象
        InitializeCelestialObjects();
        
        isInitialized = true;
        Debug.Log($"DayNightSystem初始化完成。当前月相: {currentMoonPhase}, 下一月相: {nextMoonPhase}, 状态: {visualState}");
    }

    // 获取下一个月相
    private GlobalTimeSystem.MoonPhase GetNextMoonPhase(GlobalTimeSystem.MoonPhase currentPhase)
    {
        switch (currentPhase)
        {
            case GlobalTimeSystem.MoonPhase.Crescent:
                return GlobalTimeSystem.MoonPhase.UpQuarterMoon;
            case GlobalTimeSystem.MoonPhase.UpQuarterMoon:
                return GlobalTimeSystem.MoonPhase.FullMoon;
            case GlobalTimeSystem.MoonPhase.FullMoon:
                return GlobalTimeSystem.MoonPhase.DownQuarterMoon;
            case GlobalTimeSystem.MoonPhase.DownQuarterMoon:
                return GlobalTimeSystem.MoonPhase.Crescent;
            default:
                return GlobalTimeSystem.MoonPhase.Crescent;
        }
    }

    // 开始过渡
    private void StartTransition()
    {
        blendedValue = 0f;
        visualState = VisualState.Transition;
        
        // 寻找从当前阶段到下一阶段的过渡映射
        TransitionPhaseSkyboxMapping transition = FindTransitionMapping(currentMoonPhase, nextMoonPhase);
        
        if (transition != null)
        {
            currentTransitionPhase = transition;
            UpdateSkyboxWithTransition(transition);
        }
        else
        {
            // 如果没有找到特定的过渡映射，使用静态天空盒过渡
            UpdateSkybox(); // 先设置当前天空盒
        }
        
        Debug.Log($"开始过渡: {currentMoonPhase} -> {nextMoonPhase}");
    }

    // 查找过渡映射
    private TransitionPhaseSkyboxMapping FindTransitionMapping(GlobalTimeSystem.MoonPhase fromPhase, GlobalTimeSystem.MoonPhase toPhase)
    {
        foreach (TransitionPhaseSkyboxMapping mapping in transitionPhaseMappings)
        {
            if (mapping.fromPhase == fromPhase && mapping.toPhase == toPhase)
            {
                return mapping;
            }
        }
        return null;
    }

    // 在每帧更新
    void Update()
    {
        // 确保系统已初始化
        if (!isInitialized)
        {
            InitializeSystem();
            if (!isInitialized) return; // 如果初始化失败，跳过本帧
        }

        // 检查GlobalTimeSystem实例
        if (GlobalTimeSystem.Instance == null) return;

        // 获取当前月相和进度
        GlobalTimeSystem.MoonPhase newMoonPhase = GlobalTimeSystem.Instance.CurrentPhase;
        float progress = GlobalTimeSystem.Instance.PhaseProgress;

        // 检查月相是否发生变化
        bool moonPhaseChanged = newMoonPhase != lastMoonPhase;
        if (moonPhaseChanged)
        {
            // 月相已变化，更新当前月相
            currentMoonPhase = newMoonPhase;
            lastMoonPhase = currentMoonPhase;
            
            // 重新计算下一阶段
            nextMoonPhase = GetNextMoonPhase(currentMoonPhase);
            
            // 月相变化时，如果正在过渡，则进入完成状态
            if (visualState == VisualState.Transition)
            {
                visualState = VisualState.Completed;
                blendedValue = 1f; // 确保过渡完成
                Debug.Log($"月相变化完成过渡: {currentMoonPhase}");
            }
        }

        // 状态机逻辑
        switch (visualState)
        {
            case VisualState.Stable:
                // 稳定状态下检查是否需要开始过渡
                if (progress >= TRANSITION_START_THRESHOLD)
                {
                    StartTransition();
                }
                break;
                
            case VisualState.Transition:
                // 过渡状态下更新过渡因子
                UpdateTransitionFactor(progress);
                
                // 如果月相已经变化但还没检测到，也进入完成状态
                if (moonPhaseChanged)
                {
                    visualState = VisualState.Completed;
                }
                break;
                
            case VisualState.Completed:
                // 完成状态下重置状态到稳定
                visualState = VisualState.Stable;
                blendedValue = 0f;
                UpdateSkybox(); // 更新到新的天空盒
                Debug.Log($"过渡完成，进入稳定状态: {currentMoonPhase}");
                break;
        }

        // 更新太阳位置（基于当前状态）
        UpdateSunPosition();
        
        // 更新过渡效果
        UpdateTransition();
        
        // 更新太阳和月亮对象的位置（东升西落）
        UpdateCelestialObjects();
    }

    // 更新过渡因子（基于PhaseProgress）
    private void UpdateTransitionFactor(float progress)
    {
        // 计算过渡因子：当progress在0.7~1.0之间时，从0增加到1
        if (progress >= TRANSITION_START_THRESHOLD)
        {
            blendedValue = (progress - TRANSITION_START_THRESHOLD) / (1f - TRANSITION_START_THRESHOLD);
            blendedValue = Mathf.Clamp01(blendedValue);
        }
        else
        {
            blendedValue = 0f;
        }
        
        // 更新过渡着色器的参数
        if (currentTransitionPhase != null && currentTransitionPhase.skyboxMaterial != null)
        {
            if (currentTransitionPhase.skyboxMaterial.shader != null && 
                currentTransitionPhase.skyboxMaterial.shader.name == "Custom/SkyboxTransition")
            {
                currentTransitionPhase.skyboxMaterial.SetFloat("_TransitionFactor", blendedValue);
            }
        }
    }

    // 根据当前月相和PhaseProgress选择过渡阶段
    private void SelectTransitionPhase()
    {
        // 根据当前月相确定应该播放的过渡阶段
        // 例如：在切换到新月前的半个Phase时开始播放下弦月→新月的过渡
        
        // 查找当前应该使用的过渡阶段
        TransitionPhaseSkyboxMapping selectedTransition = null;
        
        // 根据当前月相和过渡阶段的映射关系来选择
        foreach (TransitionPhaseSkyboxMapping mapping in transitionPhaseMappings)
        {
            // 这里实现完整的过渡逻辑
            if (ShouldUseTransitionPhase(mapping))
            {
                selectedTransition = mapping;
                break;
            }
        }
        
        // 如果找到了合适的过渡阶段
        if (selectedTransition != null)
        {
            currentTransitionPhase = selectedTransition;
            
            // 更新天空盒
            UpdateSkyboxWithTransition(selectedTransition);
        }
        else
        {
            // 如果没有找到过渡阶段，使用常规的天空盒更新方法
            UpdateSkybox();
        }
    }

    // 判断是否应该使用某个过渡阶段（根据月相和PhaseProgress）
    private bool ShouldUseTransitionPhase(TransitionPhaseSkyboxMapping mapping)
    {
        // 这里实现具体的逻辑来判断
        // 例如：当即将进入新月阶段时，开始播放下弦月→新月的过渡
        if (GlobalTimeSystem.Instance != null)
        {
            // 如果当前是新月阶段，且过渡是从下弦月到新月，则使用该过渡
            if (currentMoonPhase == GlobalTimeSystem.MoonPhase.Crescent && 
                mapping.fromPhase == GlobalTimeSystem.MoonPhase.DownQuarterMoon && 
                mapping.toPhase == GlobalTimeSystem.MoonPhase.Crescent)
            {
                return true;
            }
            // 其他过渡条件...
        }
        return false;
    }

    // 更新太阳位置
    private void UpdateSunPosition()
    {
        if (directionalLight == null) return;
        
        // 获取当前阶段和下一阶段的太阳角度
        float currentPhaseAngle = GetSunAngleForMoonPhase(currentMoonPhase);
        float nextPhaseAngle = GetSunAngleForMoonPhase(nextMoonPhase);
        
        // 计算目标角度（基于过渡状态）
        float targetAngle;
        if (visualState == VisualState.Transition)
        {
            // 过渡状态下进行插值
            targetAngle = Mathf.LerpAngle(currentPhaseAngle, nextPhaseAngle, blendedValue);
        }
        else
        {
            // 稳定或完成状态下使用当前阶段角度
            targetAngle = currentPhaseAngle;
        }
        
        // 平滑过渡到目标角度
        float currentSunAngle = directionalLight.transform.eulerAngles.x;
        float newSunAngle = Mathf.LerpAngle(currentSunAngle, targetAngle, Time.deltaTime * 0.5f);
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(newSunAngle, 170, 0));
    }

    // 根据月相获取太阳角度
    private float GetSunAngleForMoonPhase(GlobalTimeSystem.MoonPhase moonPhase)
    {
        switch (moonPhase)
        {
            case GlobalTimeSystem.MoonPhase.Crescent: // 夜晚
                return 270f; // 太阳在西方
            case GlobalTimeSystem.MoonPhase.UpQuarterMoon: // 日出
                return 0f; // 太阳在东方
            case GlobalTimeSystem.MoonPhase.FullMoon: // 正午
                return 90f; // 太阳在南方
            case GlobalTimeSystem.MoonPhase.DownQuarterMoon: // 日落
                return 180f; // 太阳在西方
            default:
                return 0f;
        }
    }

    // 计算连续的太阳角度（基于PhaseProgress的东升西落）
    private float CalculateContinuousSunAngle()
    {
        if (GlobalTimeSystem.Instance == null) return 0f;
        
        // 获取当前月相索引（新月=0，上弦月=1，满月=2，下弦月=3）
        int phaseIndex = (int)GlobalTimeSystem.Instance.CurrentPhase;
        float phaseProgress = GlobalTimeSystem.Instance.PhaseProgress;
        
        // 计算整个月相周期内的总进度（0~1）
        // 每个阶段占0.25，当前阶段内的进度为phaseProgress * 0.25
        float totalCycleProgress = (phaseIndex + phaseProgress) / 4f;
        
        // 将进度映射到360度：0°=东（升起），90°=南（天顶），180°=西（落下），270°=北（天底）
        float sunAngle = totalCycleProgress * 360f;
        
        return sunAngle;
    }

    // 初始化太阳和月亮对象
    private void InitializeCelestialObjects()
    {
        if (!useContinuousMotion) return;
        
        // 应用太阳材质
        if (sunObject != null && sunMaterial != null)
        {
            Renderer renderer = sunObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = sunMaterial;
            }
        }
        
        // 应用月亮材质
        if (moonObject != null && moonMaterial != null)
        {
            Renderer renderer = moonObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = moonMaterial;
            }
        }
        
        Debug.Log("太阳和月亮对象初始化完成");
    }

    // 更新太阳和月亮对象的位置
    private void UpdateCelestialObjects()
    {
        if (!useContinuousMotion) return;
        
        // 计算太阳角度
        float sunAngle = CalculateContinuousSunAngle();
        float moonAngle = (sunAngle + 180f) % 360f; // 月亮与太阳相差180度
        
        // 将角度转换为球面坐标位置
        UpdateCelestialObject(sunObject, sunAngle, sunMaterial);
        UpdateCelestialObject(moonObject, moonAngle, moonMaterial);
    }

    // 更新单个天体对象的位置和材质
    private void UpdateCelestialObject(GameObject celestialObj, float angle, Material material)
    {
        if (celestialObj == null) return;
        
        // 将角度转换为弧度
        float angleRad = angle * Mathf.Deg2Rad;
        
        // 计算球面坐标
        // 经度：angle（0°=东，90°=南，180°=西，270°=北）
        // 纬度：根据角度计算高度，0°地平线，90°天顶，180°地平线，270°天底
        float latitude = 90f - Mathf.Abs(angle - 90f); // 在0°和180°时为0°，90°时为90°
        float latitudeRad = latitude * Mathf.Deg2Rad;
        
        // 球面坐标转直角坐标
        float x = orbitRadius * Mathf.Cos(latitudeRad) * Mathf.Sin(angleRad);
        float y = orbitRadius * Mathf.Sin(latitudeRad);
        float z = orbitRadius * Mathf.Cos(latitudeRad) * Mathf.Cos(angleRad);
        
        // 设置位置（相对于轨道中心）
        celestialObj.transform.position = orbitCenter + new Vector3(x, y, z);
        
        // 始终面向轨道中心（让天体朝向场景中心）
        celestialObj.transform.LookAt(orbitCenter);
        // 调整方向，使天体的正面朝向相机（通常需要根据具体模型调整）
        celestialObj.transform.Rotate(90f, 0f, 0f);
        
        // 更新材质（如果提供了材质且对象有Renderer组件）
        if (material != null)
        {
            Renderer renderer = celestialObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }
        }
    }

    // 更新过渡效果（新系统中主要通过UpdateTransitionFactor处理）
    private void UpdateTransition()
    {
        // 在新系统中，过渡因子由UpdateTransitionFactor基于PhaseProgress计算
        // 这里可以添加其他过渡效果的处理（如雾效、光照强度等）
        
        // 确保过渡着色器的参数已更新（作为后备）
        if (visualState == VisualState.Transition && currentTransitionPhase != null && currentTransitionPhase.skyboxMaterial != null)
        {
            if (currentTransitionPhase.skyboxMaterial.shader != null && 
                currentTransitionPhase.skyboxMaterial.shader.name == "Custom/SkyboxTransition")
            {
                currentTransitionPhase.skyboxMaterial.SetFloat("_TransitionFactor", blendedValue);
            }
        }
    }

    // 更新天空盒材质的方法（使用过渡阶段）
    private void UpdateSkyboxWithTransition(TransitionPhaseSkyboxMapping transitionPhase)
    {
        Material currentSkybox = transitionPhase.skyboxMaterial;
        
        if (currentSkybox != null)
        {
            if (currentSkybox.shader != null)
            {
                if (currentSkybox.shader.name == "Custom/SkyboxTransition")
                {
                    blendedValue = 0f; // 重置过渡值
                    currentSkybox.SetFloat("_TransitionFactor", blendedValue);
                }
                else
                {
                    blendedValue = 0;
                }
            }
            RenderSettings.skybox = currentSkybox;
        }
    }

    // 原来的UpdateSkybox方法保持不变（兼容旧逻辑）
    private void UpdateSkybox()
    {
        // 寻找当前月相对应的天空盒材质
        Material currentSkybox = null;
        foreach (MoonPhaseSkyboxMapping mapping in moonPhaseMappings)
        {
            if (currentMoonPhase == mapping.moonPhase)
            {
                currentSkybox = mapping.skyboxMaterial;
                if (currentSkybox.shader != null)
                {
                    if (currentSkybox.shader.name == "Custom/SkyboxTransition")
                    {
                        blendedValue = 0f; // 重置过渡值
                        currentSkybox.SetFloat("_TransitionFactor", blendedValue);
                    }
                    else
                    {
                        blendedValue = 0;
                    }
                }
                break;
            }
        }

        // 如果找到了对应的天空盒材质，则更新RenderSettings的skybox
        if (currentSkybox != null)
        {
            RenderSettings.skybox = currentSkybox;
        }
    }
}

// 存储过渡阶段和对应天空盒材质的类
[System.Serializable]
public class TransitionPhaseSkyboxMapping
{
    public string phaseName;
    public GlobalTimeSystem.MoonPhase fromPhase; // 起始月相
    public GlobalTimeSystem.MoonPhase toPhase;   // 目标月相
    public Material skyboxMaterial; // 对应的天空盒材质
}

// 存储月相和对应天空盒材质的类（保持兼容）
[System.Serializable]
public class MoonPhaseSkyboxMapping
{
    public string phaseName;
    public GlobalTimeSystem.MoonPhase moonPhase; // 月相
    public Material skyboxMaterial; // 对应的天空盒材质
}


