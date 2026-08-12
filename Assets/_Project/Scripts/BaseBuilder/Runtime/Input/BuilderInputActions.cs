// Manual Input Action wrapper for Base Builder map switching.
// Player map = normal/strategy play context; Builder map = placement context.
// Camera pan/zoom is gated separately via StrategyCameraController.xinputs.

using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input action collection with Player and Builder maps.
/// Instantiate with <c>new BuilderInputActions()</c>, then Enable/Disable maps
/// when entering or leaving build placement modes.
/// </summary>
public class BuilderInputActions : IDisposable
{
    public InputActionAsset asset { get; }

    private readonly InputActionMap m_Player;
    private readonly InputActionMap m_Builder;

    public InputActionMap Player => m_Player;
    public InputActionMap Builder => m_Builder;

    public BuilderInputActions()
    {
        asset = InputActionAsset.FromJson(kJson);

        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Builder = asset.FindActionMap("Builder", throwIfNotFound: true);

        // Default: Player context active, Builder idle
        m_Player.Enable();
        m_Builder.Disable();
    }

    public void Dispose()
    {
        if (asset == null) return;

        asset.Disable();
        UnityEngine.Object.Destroy(asset);
    }

    // Minimal maps so Enable/Disable switching works. Placement still uses Mouse.current
    // in BaseBuilderController / PaintAndDragSystem; expand Builder actions as needed.
    private const string kJson = @"{
    ""version"": 1,
    ""name"": ""BuilderInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""3645202b-9b03-4c91-8399-8a0e7cb87d3e"",
            ""actions"": [
                {
                    ""name"": ""Point"",
                    ""type"": ""Value"",
                    ""id"": ""23ae8eb8-64f8-4d6a-b15c-e8c4f8c6a2e9"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""682e4599-4d96-4ca2-9ae7-2d5444f25165"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Builder"",
            ""id"": ""20cd83fd-8b04-49c1-88dc-1bf7a3c15a38"",
            ""actions"": [
                {
                    ""name"": ""Point"",
                    ""type"": ""Value"",
                    ""id"": ""c8a8cdef-6b77-4243-9088-7b4fc1f88443"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Place"",
                    ""type"": ""Button"",
                    ""id"": ""66a3399a-ae76-433f-87ac-006a10998d19"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Cancel"",
                    ""type"": ""Button"",
                    ""id"": ""b10dff5f-7447-4ad2-b309-9f0a5fe449d4"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""87ad1fa3-0b84-4222-af9d-e2a9ad9c6ec9"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Point"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bb0f6ddf-f574-47cc-b51f-2500645fec4a"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Place"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f2d5304f-5c30-4153-88d5-2e669d92b770"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""43642e65-d98d-46e6-bd47-f3a5a8323e30"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Cancel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}";
}
