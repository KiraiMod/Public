﻿using System;
using UnityEngine;
using UnityEngine.UI;
using static KiraiMod.Managers.GUIManager;

namespace KiraiMod.GUI
{
    public static class Movement
    {
        public static Toggle flight;
        public static Toggle directional;
        public static Toggle noclip;

        public static KiraiSlider flightSpeed;

        public static void Setup(Transform self)
        {
            Common.Window.Create(self);

            Transform Body = self.Find("Body");

            (flight = Body.Find("Flight").GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.State = state));
            (directional = Body.Find("Directional").GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.directional.Value = state));
            (noclip = Body.Find("NoClip").GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.noclip.Value = state));
            (flightSpeed = Body.Find("FlightSpeed").GetKiraiSlider()).slider.onValueChanged.AddListener(new Action<float>(value => Modules.Flight.speed.Value = value));

            Modules.Flight.directional.GUIBind(directional);
            Modules.Flight.noclip.GUIBind(noclip);
            Modules.Flight.speed.GUIBind(flightSpeed);
        }
    }
}