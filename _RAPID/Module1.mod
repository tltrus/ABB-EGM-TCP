MODULE Module1
    VAR egmident egm_id_tcp;
    LOCAL CONST jointtarget home_pos:=[[0,0,0,0,30,0],[9E9,9E9,9E9,9E9,9E9,9E9]];
    PERS wobjdata egm_wobj:=[FALSE,TRUE,"",[[0,0,0],[1,0,0,0]],[[0,0,0],[1,0,0,0]]];
    PERS tooldata current_tool:=[TRUE,[[0,0,0],[1,0,0,0]],[0.001,[0,0,0.001],[1,0,0,0],0,0,0]];

    ! ??????? ?????????
    LOCAL CONST egm_minmax pose_limits:=[-0.5,0.5];

    LOCAL CONST pose egm_correction_frame:=[[0,0,0],[1,0,0,0]];
    LOCAL CONST pose egm_sensor_frame:=[[0,0,0],[1,0,0,0]];

    PROC main()
        MoveAbsJ home_pos,v200,fine,current_tool;
        InitJointEGM;
        WHILE TRUE DO
            TPWrite "?????? EGM...";
            EGMRunPose egm_id_tcp, 
                        EGM_STOP_RAMP_DOWN, 
                        \X \Y \Z \Rx \Ry \Rz 
                        \CondTime:=1000 
                        \RampInTime:=0.05 
                        \RampOutTime:=0.5 
                        \PosCorrGain:=1.0;
            WaitTime 0.01;
        ENDWHILE
    ENDPROC

    PROC InitJointEGM()
        EGMReset egm_id_tcp;
        EGMGetId egm_id_tcp;

        EGMSetupUC ROB_1,egm_id_tcp,"default","UCdevice1",\Pose\CommTimeout:=5.0;

        EGMActPose egm_id_tcp,
                       \WObj:=egm_wobj,
                       egm_correction_frame,
                       EGM_FRAME_BASE,
                       egm_sensor_frame,
                       EGM_FRAME_BASE
                       \X:=pose_limits
                       \Y:=pose_limits
                       \Z:=pose_limits
                       \Rx:=pose_limits
                       \Ry:=pose_limits
                       \Rz:=pose_limits
                       \LpFilter:=5
                       \SampleRate:=16
                       \MaxPosDeviation:=1000
                       \MaxSpeedDeviation:=30;

    ENDPROC

    PROC StopAll()
        EGMReset egm_id_tcp;
    ENDPROC
ENDMODULE